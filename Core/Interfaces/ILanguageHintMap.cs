// Copilot作成
namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// ファイル拡張子からMarkdown言語ヒントを取得する機能を表します。
/// </summary>
public interface ILanguageHintMap
{
	/// <summary>
	/// 指定されたファイルパスに対応する言語ヒントを取得します。
	/// </summary>
	bool TryGetFileType(string filePath, out string fileType);
}
