// Copilot作成
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// 処理中に発生した警告情報を表します。
/// </summary>
public sealed class ProcessingWarning
{
	public WarningType WarningType { get; set; }
	public string Path { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string? ExceptionMessage { get; set; }
}
