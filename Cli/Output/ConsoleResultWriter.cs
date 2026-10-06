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
	public void WriteHelp( )
	{
		Console.WriteLine("source-to-markdown <input-path> <output-path> [options]");
		Console.WriteLine("  --reverse              Markdownからディレクトリーを復元します。");
		Console.WriteLine("  --language-map <path>  拡張子と言語ヒントの対応JSONを指定します。");
		Console.WriteLine("  --verbose              詳細ログを表示します。");
		Console.WriteLine("  --force                既存ファイルを確認なしで上書きします。");
		Console.WriteLine("  --help                 ヘルプを表示します。");
	}

	/// <summary>
	/// 集約処理結果を表示します。
	/// </summary>
	/// <param name="result">集約処理結果</param>
	/// <param name="options">実行オプション</param>
	public void WriteResult(ProcessingResult result, CommandLineOptions options)
	{
		Console.WriteLine("処理が完了しました。");
		Console.WriteLine($"出力ファイル: {options.OutputPath}");
		Console.WriteLine($"出力ファイル数: {result.WrittenFileCount}");
		Console.WriteLine($"スキップファイル数: {result.SkippedFileCount}");
		Console.WriteLine($"既定除外ファイル数: {result.DefaultExcludedFileCount}");
		Console.WriteLine($".gitignore除外ファイル数: {result.GitIgnoreExcludedFileCount}");
		foreach (ProcessingWarning warning in result.Warnings)
		{
			Console.Error.WriteLine($"警告: {warning.WarningType} {warning.Path} {warning.Message} {warning.ExceptionMessage}");
		}
	}

	/// <summary>
	/// 復元処理結果を表示します。
	/// </summary>
	/// <param name="result">復元処理結果</param>
	/// <param name="options">実行オプション</param>
	public void WriteReverseResult(MarkdownToSourceResult result, CommandLineOptions options)
	{
		Console.WriteLine("復元処理が完了しました。");
		Console.WriteLine($"入力Markdown: {options.InputPath}");
		Console.WriteLine($"復元先ディレクトリー: {options.OutputPath}");
		Console.WriteLine($"復元ファイル数: {result.RestoredFileCount}");
	}

	/// <summary>
	/// エラーメッセージを表示します。
	/// </summary>
	/// <param name="message">エラーメッセージ</param>
	public void WriteError(string message)
	{
		Console.Error.WriteLine($"エラー: {message}");
		Log.Error("エラー: {Message}", message);
	}
}
