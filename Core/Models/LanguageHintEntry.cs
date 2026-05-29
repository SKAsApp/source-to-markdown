// Copilot作成
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// 拡張子と言語ヒントの対応を表します。
/// </summary>
public sealed class LanguageHintEntry
{
	public string Extension { get; set; } = string.Empty;
	public string FileType { get; set; } = string.Empty;
}
