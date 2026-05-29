// Copilot作成
namespace SourceToMarkdown.Core.Enums;

/// <summary>
/// 処理中に発生する警告種別を表します。
/// </summary>
public enum WarningType
{
	SkippedByIgnoreRule,
	SkippedHiddenPath,
	SkippedSymbolicLink,
	SkippedDefaultExcludedDirectory,
	SkippedUnknownExtension,
	SkippedTooLarge,
	SkippedUnreadable,
	SkippedEncodingUnknown
}
