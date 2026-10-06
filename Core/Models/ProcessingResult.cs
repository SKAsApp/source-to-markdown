// Copilot作成
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// Markdown集約処理の結果を表します。
/// </summary>
public sealed class ProcessingResult
{
	/// <summary>出力ファイル数</summary>
	public int WrittenFileCount { get; set; }

	/// <summary>スキップファイル数</summary>
	public int SkippedFileCount { get; set; }

	/// <summary>既定除外ファイル数</summary>
	public int DefaultExcludedFileCount { get; set; }

	/// <summary>.gitignore除外ファイル数</summary>
	public int GitIgnoreExcludedFileCount { get; set; }

	/// <summary>警告一覧</summary>
	public IReadOnlyList<ProcessingWarning> Warnings { get; set; } = Array.Empty<ProcessingWarning>();

	/// <summary>終了コード</summary>
	public ExitCode ExitCode { get; set; }
}
