// Copilot作成
using SourceToMarkdown.Core.Models;
using SourceToMarkdown.Infrastructure.Markdown;
using Xunit;
namespace SourceToMarkdown.Tests;

/// <summary>
/// MarkdownRepositoryParserのテストを提供します。
/// </summary>
public sealed class MarkdownRepositoryParserTests
{
	/// <summary>
	/// 言語ヒントなしのファイルブロックを解析できることを確認します。
	/// </summary>
	[Fact]
	public void Parse_ReturnsBlockWithoutLanguageHint( )
	{
		string temporaryFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".md");
		try
		{
			File.WriteAllText(temporaryFilePath, "# source\n\n`LICENSE`\n\n~~~\n本文\n~~~\n");
			IReadOnlyList<MarkdownFileBlock> blocks = new MarkdownRepositoryParser().Parse(temporaryFilePath);
			Assert.Single(blocks);
			Assert.Equal("LICENSE", blocks[0].RelativePath);
			Assert.Equal("本文", blocks[0].Content);
		}
		finally
		{
			if (File.Exists(temporaryFilePath))
			{
				File.Delete(temporaryFilePath);
			}
		}
	}
}
