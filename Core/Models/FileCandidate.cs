// Copilot作成
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// Markdown出力候補となるファイル情報を表します。
/// </summary>
public sealed class FileCandidate
{
	public string FullPath { get; set; } = string.Empty;
	public string RelativePath { get; set; } = string.Empty;
}
