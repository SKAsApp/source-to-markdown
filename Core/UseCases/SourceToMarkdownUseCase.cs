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
	private const long MaxFileSizeBytes = 50L * 1024L * 1024L;
	private readonly IFileCollector fileCollector;
	private readonly IIgnoreRuleEvaluator ignoreRuleEvaluator;
	private readonly ILanguageHintMap languageHintMap;
	private readonly ITextFileDetector textFileDetector;
	private readonly IFileContentReader fileContentReader;
	private readonly WarningCollector warningCollector;
	private readonly ExitCodeResolver exitCodeResolver;

	/// <summary>
	/// 必要な依存機能を受け取り、ユースケースを初期化します。
	/// </summary>
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
	public ProcessingResult Execute(CommandLineOptions options)
	{
		int writtenFileCount = 0;
		int skippedFileCount = 0;
		Log.Information("Markdown集約処理を開始します。SourceDirectory={SourceDirectory} OutputMarkdown={OutputMarkdown}", options.SourceDirectoryPath, options.OutputMarkdownPath);
		using IMarkdownWriter markdownWriter = new MarkdownWriter(options.OutputMarkdownPath);
		markdownWriter.WriteHeader(options.SourceDirectoryPath);

		foreach (FileCandidate candidate in this.fileCollector.Collect(options.SourceDirectoryPath, options.OutputMarkdownPath))
		{
			this.WriteVerbose(options, $"処理中: {candidate.RelativePath}");
			Log.Debug("ファイル処理を開始します。Path={RelativePath}", candidate.RelativePath);
			IgnoreDecision ignoreDecision = this.ignoreRuleEvaluator.Evaluate(options.SourceDirectoryPath, candidate.RelativePath);
			if (ignoreDecision.IsIgnored)
			{
				skippedFileCount++;
				this.warningCollector.Add(ignoreDecision.WarningType ?? WarningType.SkippedByIgnoreRule, candidate.RelativePath, ignoreDecision.Reason);
				this.WriteVerbose(options, $"スキップ: {candidate.RelativePath} 理由: {ignoreDecision.Reason}");
				Log.Information("ファイルをスキップしました。Path={RelativePath} Reason={Reason}", candidate.RelativePath, ignoreDecision.Reason);
				continue;
			}

			if (!this.languageHintMap.TryGetFileType(candidate.RelativePath, out string fileType))
			{
				skippedFileCount++;
				this.warningCollector.Add(WarningType.SkippedUnknownExtension, candidate.RelativePath, "未知の拡張子です。");
				this.WriteVerbose(options, $"スキップ: {candidate.RelativePath} 理由: 未知の拡張子です。");
				Log.Information("未知の拡張子のためスキップしました。Path={RelativePath}", candidate.RelativePath);
				continue;
			}

			try
			{
				TextDetectionResult detectionResult = this.textFileDetector.Detect(candidate.FullPath, MaxFileSizeBytes);
				if (!detectionResult.IsText || detectionResult.Encoding == null)
				{
					skippedFileCount++;
					this.warningCollector.Add(detectionResult.WarningType ?? WarningType.SkippedEncodingUnknown, candidate.RelativePath, detectionResult.Message);
					this.WriteVerbose(options, $"スキップ: {candidate.RelativePath} 理由: {detectionResult.Message}");
					Log.Information("テキスト判定によりスキップしました。Path={RelativePath} Reason={Reason}", candidate.RelativePath, detectionResult.Message);
					continue;
				}

				this.WriteVerbose(options, $"採用文字コード: {candidate.RelativePath} {detectionResult.Encoding.EncodingName}");
				this.WriteVerbose(options, $"採用言語ヒント: {candidate.RelativePath} {fileType}");
				Log.Debug("ファイルを書き出します。Path={RelativePath} Encoding={Encoding} FileType={FileType}", candidate.RelativePath, detectionResult.Encoding.EncodingName, fileType);
				string content = this.fileContentReader.ReadAllTextNormalized(candidate.FullPath, detectionResult.Encoding);
				markdownWriter.WriteFileBlock(candidate.RelativePath, fileType, content);
				writtenFileCount++;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
			{
				skippedFileCount++;
				this.warningCollector.Add(WarningType.SkippedUnreadable, candidate.RelativePath, "ファイルを読み取れません。", exception);
				this.WriteVerbose(options, $"スキップ: {candidate.RelativePath} 理由: ファイルを読み取れません。 {exception.Message}");
				Log.Warning(exception, "ファイルを読み取れないためスキップしました。Path={RelativePath}", candidate.RelativePath);
			}
		}

		IReadOnlyList<ProcessingWarning> warnings = this.warningCollector.GetWarnings();
		Log.Information("Markdown集約処理を終了します。Written={WrittenFileCount} Skipped={SkippedFileCount} WarningCount={WarningCount}", writtenFileCount, skippedFileCount, warnings.Count);
		return new ProcessingResult { WrittenFileCount = writtenFileCount, SkippedFileCount = skippedFileCount, Warnings = warnings, ExitCode = this.exitCodeResolver.Resolve(warnings.Count > 0) };
	}

	/// <summary>
	/// verbose指定時に詳細ログを標準エラーへ出力します。
	/// </summary>
	private void WriteVerbose(CommandLineOptions options, string message)
	{
		if (!options.Verbose)
		{
			return;
		}

		Console.Error.WriteLine($"詳細: {message}");
	}
}
