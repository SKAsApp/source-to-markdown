// Copilot作成
using Serilog;
using SourceToMarkdown.Cli.Options;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Cli.Output;

/// <summary>
/// コンソールへヘルプ、結果、警告を出力します。
/// </summary>
public sealed class ConsoleResultWriter
{
	/// <summary>
	/// ヘルプメッセージを表示します。
	/// </summary>
	public void WriteHelp()
	{
		Console.WriteLine("source-to-markdown <source-directory> <output-markdown> [options]");
		Console.WriteLine("Options:");
		Console.WriteLine("  --language-map <path>  拡張子と言語ヒントの対応JSONを指定します。");
		Console.WriteLine("  --verbose              詳細ログを表示します。");
		Console.WriteLine("  --force                既存ファイルを確認なしで上書きします。");
		Console.WriteLine("  --help                 ヘルプを表示します。");
	}

	/// <summary>
	/// 処理結果を表示します。
	/// </summary>
	public void WriteResult(ProcessingResult result, CommandLineOptions options)
	{
		Console.WriteLine("処理が完了しました。");
		Console.WriteLine($"出力ファイル: {options.OutputMarkdownPath}");
		Console.WriteLine($"出力ファイル数: {result.WrittenFileCount}");
		Console.WriteLine($"スキップファイル数: {result.SkippedFileCount}");
		Log.Information("処理結果を出力しました。Output={OutputMarkdownPath} Written={WrittenFileCount} Skipped={SkippedFileCount}", options.OutputMarkdownPath, result.WrittenFileCount, result.SkippedFileCount);
		foreach (ProcessingWarning warning in result.Warnings)
		{
			Console.Error.WriteLine($"警告: {warning.WarningType} {warning.Path} {warning.Message} {warning.ExceptionMessage}");
			Log.Warning("警告: Type={WarningType} Path={Path} Message={Message} Exception={ExceptionMessage}", warning.WarningType, warning.Path, warning.Message, warning.ExceptionMessage);
		}
	}

	/// <summary>
	/// エラーメッセージを表示します。
	/// </summary>
	public void WriteError(string message)
	{
		Console.Error.WriteLine($"エラー: {message}");
		Log.Error("エラー: {Message}", message);
	}
}
