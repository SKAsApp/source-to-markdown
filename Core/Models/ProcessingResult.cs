// Copilot作成
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// Markdown集約処理の結果を表します。
/// </summary>
public sealed class ProcessingResult
{
	public int WrittenFileCount { get; set; }
	public int SkippedFileCount { get; set; }
	public IReadOnlyList<ProcessingWarning> Warnings { get; set; } = Array.Empty<ProcessingWarning>();
	public ExitCode ExitCode { get; set; }
}
