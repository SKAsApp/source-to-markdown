// Copilot作成
using Serilog;
using SourceToMarkdown.Cli.Options;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;
using SourceToMarkdown.Infrastructure.Markdown;

namespace SourceToMarkdown.Core.UseCases;

/// <summary>
/// ソースコードをMarkdownへ集約するユースケースを実行します。
/// </summary>
public sealed class SourceToMarkdownUseCase
{
	/// <summary>処理可能な最大ファイルサイズ</summary>
	private const long MaxFileSizeBytes = 50L * 1024L * 1024L;

	/// <summary>候補ファイルを収集する機能</summary>
	private readonly IFileCollector fileCollector;

	/// <summary>除外規則を評価する機能</summary>
	private readonly IIgnoreRuleEvaluator ignoreRuleEvaluator;

	/// <summary>言語ヒントを取得する機能</summary>
	private readonly ILanguageHintMap languageHintMap;

	/// <summary>テキストファイルを判定する機能</summary>
	private readonly ITextFileDetector textFileDetector;

	/// <summary>ファイル本文を読み込む機能</summary>
	private readonly IFileContentReader fileContentReader;

	/// <summary>警告を収集する機能</summary>
	private readonly WarningCollector warningCollector;

	/// <summary>終了コードを決定する機能</summary>
	private readonly ExitCodeResolver exitCodeResolver;

	/// <summary>
	/// 必要な依存機能を受け取り、ユースケースを初期化します。
	/// </summary>
	/// <param name="fileCollector">候補ファイルを収集する機能</param>
	/// <param name="ignoreRuleEvaluator">除外規則を評価する機能</param>
	/// <param name="languageHintMap">言語ヒントを取得する機能</param>
	/// <param name="textFileDetector">テキストファイルを判定する機能</param>
	/// <param name="fileContentReader">ファイル本文を読み込む機能</param>
	/// <param name="warningCollector">警告を収集する機能</param>
	/// <param name="exitCodeResolver">終了コードを決定する機能</param>
	public SourceToMarkdownUseCase(IFileCollector fileCollector, IIgnoreRuleEvaluator ignoreRuleEvaluator, ILanguageHintMap languageHintMap, ITextFileDetector textFileDetector, IFileContentReader fileContentReader, WarningCollector warningCollector, ExitCodeResolver exitCodeResolver)
	{
		this.fileCollector = fileCollector;
		this.ignoreRuleEvaluator = ignoreRuleEvaluator;
		this.languageHintMap = languageHintMap;
		this.textFileDetector = textFileDetector;
		this.fileContentReader = fileContentReader;
		this.warningCollector = warningCollector;
		this.exitCodeResolver = exitCodeResolver;
	}

	/// <summary>
	/// 指定されたオプションに従ってMarkdown集約処理を実行します。
	/// </summary>
	/// <param name="options">実行オプション</param>
	/// <returns>集約処理結果</returns>
	public ProcessingResult Execute(CommandLineOptions options)
	{
		int writtenFileCount = 0;
		int skippedFileCount = 0;
		int defaultExcludedFileCount = 0;
		int gitIgnoreExcludedFileCount = 0;
		using IMarkdownWriter markdownWriter = new MarkdownWriter(options.OutputPath);
		markdownWriter.WriteHeader(options.InputPath);
		foreach (FileCandidate candidate in this.fileCollector.Collect(options.InputPath, options.OutputPath))
		{
			IgnoreDecision ignoreDecision = this.ignoreRuleEvaluator.Evaluate(options.InputPath, candidate.RelativePath);
			if (ignoreDecision.IsIgnored)
			{
				skippedFileCount++;
				if (ignoreDecision.WarningType == WarningType.SkippedDefaultExcludedDirectory)
				{
					defaultExcludedFileCount++;
				}
				else if (ignoreDecision.WarningType == WarningType.SkippedByIgnoreRule)
				{
					gitIgnoreExcludedFileCount++;
				}
				else
				{
					// 除外種別が不明な場合は、正常な.gitignore除外として扱わず確認対象の警告にします。
					this.warningCollector.Add(ignoreDecision.WarningType ?? WarningType.SkippedEncodingUnknown, candidate.RelativePath, ignoreDecision.Reason);
				}
				this.WriteVerbose(options, $"スキップ: {candidate.RelativePath} 理由: {ignoreDecision.Reason}");
				Log.Information("ファイルをスキップしました。Path={RelativePath} Reason={Reason}", candidate.RelativePath, ignoreDecision.Reason);
				continue;
			}
			try
			{
				TextDetectionResult detectionResult = this.textFileDetector.Detect(candidate.FullPath, MaxFileSizeBytes);
				if (!detectionResult.IsText || detectionResult.Encoding == null)
				{
					skippedFileCount++;
					this.warningCollector.Add(detectionResult.WarningType ?? WarningType.SkippedEncodingUnknown, candidate.RelativePath, detectionResult.Message);
					continue;
				}
				string fileType = string.Empty;
				this.languageHintMap.TryGetFileType(candidate.RelativePath, out fileType);
				string content = this.fileContentReader.ReadAllTextNormalized(candidate.FullPath, detectionResult.Encoding);
				markdownWriter.WriteFileBlock(candidate.RelativePath, fileType ?? string.Empty, content);
				writtenFileCount++;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
			{
				skippedFileCount++;
				this.warningCollector.Add(WarningType.SkippedUnreadable, candidate.RelativePath, "ファイルを読み取れません。", exception);
			}
		}
		IReadOnlyList<ProcessingWarning> warnings = this.warningCollector.GetWarnings();
		return new ProcessingResult
		{
			WrittenFileCount = writtenFileCount,
			SkippedFileCount = skippedFileCount,
			DefaultExcludedFileCount = defaultExcludedFileCount,
			GitIgnoreExcludedFileCount = gitIgnoreExcludedFileCount,
			Warnings = warnings,
			ExitCode = this.exitCodeResolver.Resolve(warnings.Count > 0)
		};
	}

	/// <summary>
	/// 詳細表示が有効な場合にメッセージを出力します。
	/// </summary>
	/// <param name="options">実行オプション</param>
	/// <param name="message">表示メッセージ</param>
	private void WriteVerbose(CommandLineOptions options, string message)
	{
		if (!options.Verbose)
		{
			return;
		}
		Console.Error.WriteLine($"詳細: {message}");
	}
}
