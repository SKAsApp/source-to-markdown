// Copilot作成
namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// Markdownを書き出す機能を表します。
/// </summary>
public interface IMarkdownWriter : IDisposable
{
	/// <summary>
	/// Markdownファイルのヘッダーを書き込みます。
	/// </summary>
	void WriteHeader(string sourceDirectoryArgument);

	/// <summary>
	/// 1ファイル分のMarkdownブロックを書き込みます。
	/// </summary>
	void WriteFileBlock(string relativePath, string fileType, string content);
}
