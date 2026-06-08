// Copilot作成
using SourceToMarkdown.Infrastructure.Markdown;

using Xunit;

namespace SourceToMarkdown.Tests;

/// <summary>
/// MarkdownWriterのテストを提供します。
/// </summary>
public sealed class MarkdownWriterTests
{
	/// <summary>
	/// 最後のファイルブロック後に区切り線が出力されないことを確認します。
	/// </summary>
	[Fact]
	public void WriteFileBlock_DoesNotWriteTrailingSeparator()
	{
		string temporaryFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".md");
		try
		{
			using (MarkdownWriter writer = new MarkdownWriter(temporaryFilePath))
			{
				writer.WriteHeader("source");
				writer.WriteFileBlock("a.cs", "csharp", "class A { }");
			}

			string markdown = File.ReadAllText(temporaryFilePath);
			Assert.False(markdown.TrimEnd().EndsWith("---", StringComparison.Ordinal));
		}
		finally
		{
			if (File.Exists(temporaryFilePath))
			{
				File.Delete(temporaryFilePath);
			}
		}
	}

	/// <summary>
	/// 複数ファイルの間だけ区切り線が出力されることを確認します。
	/// </summary>
	[Fact]
	public void WriteFileBlock_WritesSeparatorBetweenFiles()
	{
		string temporaryFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".md");
		try
		{
			using (MarkdownWriter writer = new MarkdownWriter(temporaryFilePath))
			{
				writer.WriteHeader("source");
				writer.WriteFileBlock("a.cs", "csharp", "class A { }");
				writer.WriteFileBlock("b.cs", "csharp", "class B { }");
			}

			string markdown = File.ReadAllText(temporaryFilePath);
			Assert.Contains(Environment.NewLine + "---" + Environment.NewLine, markdown);
			Assert.False(markdown.TrimEnd().EndsWith("---", StringComparison.Ordinal));
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
