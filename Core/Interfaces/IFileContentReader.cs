// Copilot作成
using System.Text;

namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// ファイル本文を読み込む機能を表します。
/// </summary>
public interface IFileContentReader
{
	/// <summary>
	/// 指定された文字コードでファイルを読み込み、改行をLFへ正規化します。
	/// </summary>
	string ReadAllTextNormalized(string filePath, Encoding encoding);
}
