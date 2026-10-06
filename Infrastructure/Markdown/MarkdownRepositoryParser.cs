// Copilot作成
using System.Text;
using System.Text.RegularExpressions;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;
namespace SourceToMarkdown.Infrastructure.Markdown;

/// <summary>
/// 本ツールが出力したMarkdownから認識可能なファイルブロックを抽出します。
/// </summary>
public sealed class MarkdownRepositoryParser : IMarkdownRepositoryParser
{
	private static readonly Regex PathLinePattern = new Regex(@"^`([^`]+)`$", RegexOptions.CultureInvariant);
	private static readonly Regex FencePattern = new Regex(@"^(`{3,}|~{3,})[^`~]*$", RegexOptions.CultureInvariant);

	/// <summary>
	/// Markdownを解析します。
	/// </summary>
	/// <param name="markdownPath">入力Markdownファイルのパス</param>
	/// <returns>認識できたファイルブロックの一覧</returns>
	public IReadOnlyList<MarkdownFileBlock> Parse(string markdownPath)
	{
		string markdown = File.ReadAllText(markdownPath, new UTF8Encoding(false, true)).Replace("\r\n", "\n").Replace("\r", "\n");
		string[] lines = markdown.Split('\n');
		List<MarkdownFileBlock> blocks = new List<MarkdownFileBlock>();
		for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
		{
			Match pathMatch = PathLinePattern.Match(lines[lineIndex]);
			if (!pathMatch.Success)
			{
				continue;
			}
			int fenceLineIndex = lineIndex + 1;
			while (fenceLineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[fenceLineIndex]))
			{
				fenceLineIndex++;
			}
			if (fenceLineIndex >= lines.Length)
			{
				continue;
			}
			Match fenceMatch = FencePattern.Match(lines[fenceLineIndex]);
			if (!fenceMatch.Success)
			{
				continue;
			}
			string fence = fenceMatch.Groups[1].Value;
			int closingLineIndex = fenceLineIndex + 1;
			while (closingLineIndex < lines.Length && lines[closingLineIndex] != fence)
			{
				closingLineIndex++;
			}
			if (closingLineIndex >= lines.Length)
			{
				continue;
			}
			string content = string.Join("\n", lines[(fenceLineIndex + 1)..closingLineIndex]);
			blocks.Add(new MarkdownFileBlock
			{
				RelativePath = pathMatch.Groups[1].Value,
				Content = content
			});
			lineIndex = closingLineIndex;
		}
		return blocks;
	}
}
