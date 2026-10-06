// Copilot作成
namespace SourceToMarkdown.Core.Enums;

/// <summary>
/// 処理中に発生する警告種別を表します。
/// </summary>
public enum WarningType
{
	/// <summary>.gitignoreによる除外。警告一覧には追加せず件数として集計</summary>
	SkippedByIgnoreRule,

	/// <summary>隠しパスによる除外</summary>
	SkippedHiddenPath,

	/// <summary>シンボリックリンクによる除外</summary>
	SkippedSymbolicLink,

	/// <summary>既定除外ディレクトリーによる除外。警告一覧には追加せず件数として集計</summary>
	SkippedDefaultExcludedDirectory,

	/// <summary>互換性維持用の未登録拡張子による除外</summary>
	SkippedUnknownExtension,

	/// <summary>ファイルサイズ超過による除外</summary>
	SkippedTooLarge,

	/// <summary>読み取り不能による除外</summary>
	SkippedUnreadable,

	/// <summary>文字コード判定不能による除外</summary>
	SkippedEncodingUnknown
}
