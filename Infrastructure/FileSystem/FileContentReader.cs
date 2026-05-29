// Copilot作成
using System.Text;
using SourceToMarkdown.Core.Interfaces;

namespace SourceToMarkdown.Infrastructure.FileSystem;

/// <summary>
/// ファイル本文を読み込み、改行を正規化します。
/// </summary>
public sealed class FileContentReader : IFileContentReader
{
	/// <summary>
	/// 指定された文字コードでファイルを読み込み、改行をLFへ正規化します。
	/// </summary>
	public string ReadAllTextNormalized(string filePath, Encoding encoding)
	{
		string content = File.ReadAllText(filePath, encoding);
		return content.Replace("
", "
").Replace("", "
");
	}
}
