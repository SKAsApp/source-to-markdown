// Copilot作成
using SourceToMarkdown.Core.Models;
namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// 本ツール形式のMarkdownからファイルブロックを抽出します。
/// </summary>
public interface IMarkdownRepositoryParser
{
	/// <summary>
	/// Markdownを解析します。
	/// </summary>
	/// <param name="markdownPath">入力Markdownファイルのパス</param>
	/// <returns>認識できたファイルブロックの一覧</returns>
	IReadOnlyList<MarkdownFileBlock> Parse(string markdownPath);
}
