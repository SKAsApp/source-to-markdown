// Copilot作成
using System.Text;
using SourceToMarkdown.Core.Interfaces;

namespace SourceToMarkdown.Infrastructure.Markdown;

/// <summary>
/// Markdownファイルへコードブロックを書き込みます。
/// </summary>
public sealed class MarkdownWriter : IMarkdownWriter
{
	private readonly StreamWriter writer;

	/// <summary>
	/// Markdown出力先を指定してライターを初期化します。
	/// </summary>
	public MarkdownWriter(string outputMarkdownPath)
	{
		string? directoryPath = Path.GetDirectoryName(Path.GetFullPath(outputMarkdownPath));
		if (!string.IsNullOrWhiteSpace(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
		this.writer = new StreamWriter(outputMarkdownPath, false, new UTF8Encoding(false));
	}

	/// <summary>
	/// Markdownファイルのヘッダーを書き込みます。
	/// </summary>
	public void WriteHeader(string sourceDirectoryArgument)
	{
		this.writer.WriteLine($"# {sourceDirectoryArgument}");
		this.writer.WriteLine();
	}

	/// <summary>
	/// 1ファイル分のMarkdownブロックを書き込みます。
	/// </summary>
	public void WriteFileBlock(string relativePath, string fileType, string content)
	{
		string fence = ResolveFence(content);
		this.writer.WriteLine($"`{relativePath}`");
		this.writer.WriteLine($"{fence}{fileType}");
		this.writer.WriteLine(content);
		this.writer.WriteLine(fence);
		this.writer.WriteLine("---");
	}

	/// <summary>
	/// 本文と衝突しないコードフェンスを決定します。
	/// </summary>
	private static string ResolveFence(string content)
	{
		if (!content.Contains("```", StringComparison.Ordinal))
		{
			return "```";
		}
		int length = 3;
		while (content.Contains(new string('~', length), StringComparison.Ordinal))
		{
			length++;
		}
		return new string('~', length);
	}

	/// <summary>
	/// 内部ライターを破棄します。
	/// </summary>
	public void Dispose()
	{
		this.writer.Dispose();
	}
}
