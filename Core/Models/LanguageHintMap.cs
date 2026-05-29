// Copilot作成
using SourceToMarkdown.Core.Interfaces;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// 拡張子と言語ヒントの対応表を管理します。
/// </summary>
public sealed class LanguageHintMap : ILanguageHintMap
{
	private readonly Dictionary<string, string> fileTypes;

	/// <summary>
	/// 言語ヒント一覧から対応表を初期化します。
	/// </summary>
	public LanguageHintMap(IEnumerable<LanguageHintEntry> entries)
	{
		this.fileTypes = entries.ToDictionary(entry => NormalizeExtension(entry.Extension), entry => entry.FileType, StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>
	/// 指定されたファイルパスに対応する言語ヒントを取得します。
	/// </summary>
	public bool TryGetFileType(string filePath, out string fileType)
	{
		string extension = NormalizeExtension(Path.GetExtension(filePath));
		return this.fileTypes.TryGetValue(extension, out fileType!);
	}

	/// <summary>
	/// 拡張子をドットなし小文字へ正規化します。
	/// </summary>
	private static string NormalizeExtension(string extension)
	{
		return extension.Trim().TrimStart('.').ToLowerInvariant();
	}
}
