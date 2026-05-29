// Copilot作成
using System.Text;
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.Models;

/// <summary>
/// テキストファイル判定の結果を表します。
/// </summary>
public sealed class TextDetectionResult
{
	public bool IsText { get; set; }
	public Encoding? Encoding { get; set; }
	public WarningType? WarningType { get; set; }
	public string Message { get; set; } = string.Empty;
}
