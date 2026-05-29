// Copilot作成
using SourceToMarkdown.Cli.Options;
using SourceToMarkdown.Cli.Output;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Models;
using SourceToMarkdown.Core.UseCases;
using SourceToMarkdown.Infrastructure.Encoding;
using SourceToMarkdown.Infrastructure.FileSystem;
using SourceToMarkdown.Infrastructure.GitIgnore;

namespace SourceToMarkdown;

/// <summary>
/// アプリケーションのエントリーポイントを提供します。
/// </summary>
public static class Program
{
	/// <summary>
	/// コマンドライン引数を受け取り、Markdown集約処理を実行します。
	/// </summary>
	public static int Main(string[] args)
	{
		ConsoleResultWriter resultWriter = new ConsoleResultWriter();
		try
		{
			CommandLineParser parser = new CommandLineParser();
			CommandLineOptions options = parser.Parse(args);
			if (options.ShowHelp)
			{
				resultWriter.WriteHelp();
				return (int)ExitCode.Success;
			}
			if (string.IsNullOrWhiteSpace(options.SourceDirectoryPath) || string.IsNullOrWhiteSpace(options.OutputMarkdownPath))
			{
				resultWriter.WriteError("入力ディレクトリーと出力Markdownファイルを指定してください。");
				resultWriter.WriteHelp();
				return (int)ExitCode.ArgumentError;
			}
			if (!Directory.Exists(options.SourceDirectoryPath))
			{
				resultWriter.WriteError("入力ディレクトリーが存在しません。");
				return (int)ExitCode.ArgumentError;
			}
			if (File.Exists(options.OutputMarkdownPath) && !options.Force && !ConfirmOverwrite(options.OutputMarkdownPath))
			{
				resultWriter.WriteError("上書きが拒否されました。");
				return (int)ExitCode.ArgumentError;
			}

			AppSettingsLoader settingsLoader = new AppSettingsLoader();
			LanguageHintMap languageHintMap = new LanguageHintMap(settingsLoader.LoadLanguageHints(options.LanguageMapPath));
			SourceToMarkdownUseCase useCase = new SourceToMarkdownUseCase(new FileCollector(), new GitIgnoreRuleEvaluator(), languageHintMap, new TextFileDetector(), new FileContentReader(), new WarningCollector(), new ExitCodeResolver());
			ProcessingResult result = useCase.Execute(options);
			resultWriter.WriteResult(result, options);
			return (int)result.ExitCode;
		}
		catch (Exception exception)
		{
			resultWriter.WriteError(exception.Message);
			return (int)ExitCode.RuntimeError;
		}
	}

	/// <summary>
	/// 既存の出力Markdownファイルを上書きしてよいか確認します。
	/// </summary>
	private static bool ConfirmOverwrite(string outputMarkdownPath)
	{
		Console.Write($"出力ファイルが既に存在します。上書きしますか？ [y/N]: {outputMarkdownPath} ");
		string? answer = Console.ReadLine();
		return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
	}
}
