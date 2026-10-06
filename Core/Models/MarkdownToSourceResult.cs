// Copilot作成
using SourceToMarkdown.Core.Enums;
namespace SourceToMarkdown.Core.Models;

/// <summary>
/// Markdownからの復元処理結果を表します。
/// </summary>
public sealed class MarkdownToSourceResult
{
	/// <summary>復元したファイル数</summary>
	public int RestoredFileCount { get; set; }
	/// <summary>終了コード</summary>
	public ExitCode ExitCode { get; set; }
}
