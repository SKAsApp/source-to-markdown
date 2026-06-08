// Copilot作成
using SourceToMarkdown.Infrastructure.Encoding;

using Xunit;

namespace SourceToMarkdown.Tests;

/// <summary>
/// TextFileDetectorのテストを提供します。
/// </summary>
public sealed class TextFileDetectorTests
{
	/// <summary>
	/// 空ファイルがテキストとして判定されることを確認します。
	/// </summary>
	[Fact]
	public void Detect_ReturnsTextForEmptyFile()
	{
		string temporaryFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
		try
		{
			File.WriteAllBytes(temporaryFilePath, Array.Empty<byte>());
			TextFileDetector detector = new TextFileDetector();
			var result = detector.Detect(temporaryFilePath, 50L * 1024L * 1024L);
			Assert.True(result.IsText);
			Assert.NotNull(result.Encoding);
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
