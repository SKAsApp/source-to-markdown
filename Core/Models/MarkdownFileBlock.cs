// Copilot作成
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// Markdownから抽出した1ファイル分の情報を表します。
/// </summary>
public sealed class MarkdownFileBlock
{
	/// <summary>Markdownに記載された相対パス</summary>
	public string RelativePath { get; set; } = string.Empty;
	/// <summary>復元するファイル本文</summary>
	public string Content { get; set; } = string.Empty;
}
