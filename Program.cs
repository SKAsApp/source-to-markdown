// Copilot作成
using Serilog;
using SourceToMarkdown.Cli.Options;
using SourceToMarkdown.Cli.Output;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Models;
using SourceToMarkdown.Core.UseCases;
using SourceToMarkdown.Infrastructure.Encoding;
using SourceToMarkdown.Infrastructure.FileSystem;
using SourceToMarkdown.Infrastructure.GitIgnore;
using SourceToMarkdown.Infrastructure.Logging;
using SourceToMarkdown.Infrastructure.Markdown;
namespace SourceToMarkdown;

/// <summary>
/// アプリケーションのエントリーポイントを提供します。
/// </summary>
public static class Program
{
	/// <summary>
	/// コマンドライン引数を受け取り、指定されたモードを実行します。
	/// </summary>
	/// <param name="args">コマンドライン引数</param>
	/// <returns>終了コード</returns>
	public static int Main(string[] args)
	{
		ConsoleResultWriter resultWriter = new ConsoleResultWriter();
		AppSettingsLoader settingsLoader = new AppSettingsLoader();
		new SerilogConfigurator().Configure(settingsLoader.LoadApplicationSettings().Logging);
		try
		{
			CommandLineOptions options = new CommandLineParser().Parse(args);
			if (options.ShowHelp)
			{
				resultWriter.WriteHelp( );
				return (int)ExitCode.Success;
			}
			if (!string.IsNullOrWhiteSpace(options.ParseErrorMessage))
			{
				resultWriter.WriteError(options.ParseErrorMessage);
				resultWriter.WriteHelp( );
				return (int)ExitCode.ArgumentError;
			}
			if (options.Reverse)
			{
				return ExecuteReverseMode(options, resultWriter);
			}
			return ExecuteForwardMode(options, resultWriter, settingsLoader);
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "致命的なエラーが発生しました。");
			resultWriter.WriteError(exception.Message);
			return (int)ExitCode.RuntimeError;
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	/// <summary>
	/// 通常モードを実行します。
	/// </summary>
	/// <param name="options">実行オプション</param>
	/// <param name="resultWriter">結果出力機能</param>
	/// <param name="settingsLoader">設定読み込み機能</param>
	/// <returns>終了コード</returns>
	private static int ExecuteForwardMode(CommandLineOptions options, ConsoleResultWriter resultWriter, AppSettingsLoader settingsLoader)
	{
		if (!Directory.Exists(options.InputPath))
		{
			resultWriter.WriteError("入力ディレクトリーが存在しません。");
			return (int)ExitCode.ArgumentError;
		}
		if (File.Exists(options.OutputPath) && !options.Force && !ConfirmOverwrite(options.OutputPath))
		{
			resultWriter.WriteError("上書きが拒否されました。");
			return (int)ExitCode.ArgumentError;
		}
		LanguageHintMap languageHintMap = new LanguageHintMap(settingsLoader.LoadLanguageHints(options.LanguageMapPath));
		SourceToMarkdownUseCase useCase = new SourceToMarkdownUseCase(new FileCollector(), new GitIgnoreRuleEvaluator(), languageHintMap, new TextFileDetector(), new FileContentReader(), new WarningCollector(), new ExitCodeResolver());
		ProcessingResult result = useCase.Execute(options);
		resultWriter.WriteResult(result, options);
		return (int)result.ExitCode;
	}

	/// <summary>
	/// 逆モードを実行します。
	/// </summary>
	/// <param name="options">実行オプション</param>
	/// <param name="resultWriter">結果出力機能</param>
	/// <returns>終了コード</returns>
	private static int ExecuteReverseMode(CommandLineOptions options, ConsoleResultWriter resultWriter)
	{
		if (!File.Exists(options.InputPath))
		{
			resultWriter.WriteError("入力Markdownファイルが存在しません。");
			return (int)ExitCode.ArgumentError;
		}
		if (File.Exists(options.OutputPath))
		{
			resultWriter.WriteError("復元先にはディレクトリーを指定してください。");
			return (int)ExitCode.ArgumentError;
		}
		MarkdownToSourceUseCase useCase = new MarkdownToSourceUseCase(new MarkdownRepositoryParser());
		MarkdownToSourceResult result = useCase.Execute(options);
		resultWriter.WriteReverseResult(result, options);
		return (int)result.ExitCode;
	}

	/// <summary>
	/// 既存の出力Markdownファイルを上書きしてよいか確認します。
	/// </summary>
	/// <param name="outputMarkdownPath">出力Markdownファイルのパス</param>
	/// <returns>上書きを許可する場合はtrue</returns>
	private static bool ConfirmOverwrite(string outputMarkdownPath)
	{
		Console.Write($"出力ファイルが既に存在します。上書きしますか？ [y/N]: {outputMarkdownPath} ");
		string? answer = Console.ReadLine();
		return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
	}
}
