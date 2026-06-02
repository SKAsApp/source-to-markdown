// Copilot作成
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// アプリケーション全体の設定を表します。
/// </summary>
public sealed class ApplicationSettings
{
	/// <summary>
	/// ログ出力設定を取得または設定します。
	/// </summary>
	public LoggingSettings Logging { get; set; } = new LoggingSettings();
}
