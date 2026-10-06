// Copilot作成
namespace SourceToMarkdown.Cli.Options;

/// <summary>
/// コマンドライン引数から生成される実行オプションを表します。
/// </summary>
public sealed class CommandLineOptions
{
	/// <summary>入力パス</summary>
	public string InputPath { get; set; } = string.Empty;

	/// <summary>出力パス</summary>
	public string OutputPath { get; set; } = string.Empty;

	/// <summary>言語ヒント設定ファイルのパス</summary>
	public string? LanguageMapPath { get; set; }

	/// <summary>詳細表示の有効状態</summary>
	public bool Verbose { get; set; }

	/// <summary>強制上書きの有効状態</summary>
	public bool Force { get; set; }

	/// <summary>逆モードの有効状態</summary>
	public bool Reverse { get; set; }

	/// <summary>ヘルプ表示の有効状態</summary>
	public bool ShowHelp { get; set; }

	/// <summary>引数解析時のエラーメッセージ</summary>
	public string? ParseErrorMessage { get; set; }
}
