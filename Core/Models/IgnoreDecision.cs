// Copilot作成
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// 除外判定の結果を表します。
/// </summary>
public sealed class IgnoreDecision
{
	public bool IsIgnored { get; set; }
	public WarningType? WarningType { get; set; }
	public string Reason { get; set; } = string.Empty;
}
