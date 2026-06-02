// Copilot作成
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// ログ出力設定を表します。
/// </summary>
public sealed class LoggingSettings
{
	/// <summary>
	/// ログファイルを書き出すディレクトリーのパスを取得または設定します。
	/// </summary>
	public string? LogDirectoryPath { get; set; }

	/// <summary>
	/// ログ出力の最小レベルを取得または設定します。
	/// </summary>
	public string MinimumLevel { get; set; } = "Information";

	/// <summary>
	/// 保持するログファイル数を取得または設定します。
	/// </summary>
	public int RetainedFileCountLimit { get; set; } = 31;
}
