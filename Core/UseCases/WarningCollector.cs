// Copilot作成
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Core.UseCases;

/// <summary>
/// 警告情報を収集します。
/// </summary>
public sealed class WarningCollector
{
	private readonly List<ProcessingWarning> warnings = new List<ProcessingWarning>();

	/// <summary>
	/// 警告を追加します。
	/// </summary>
	public void Add(WarningType warningType, string path, string message, Exception? exception = null)
	{
		this.warnings.Add(new ProcessingWarning { WarningType = warningType, Path = path, Message = message, ExceptionMessage = exception?.Message });
	}

	/// <summary>
	/// 収集済みの警告一覧を取得します。
	/// </summary>
	public IReadOnlyList<ProcessingWarning> GetWarnings()
	{
		return this.warnings;
	}

	/// <summary>
	/// 警告が存在するかどうかを取得します。
	/// </summary>
	public bool HasWarnings()
	{
		return this.warnings.Count > 0;
	}
}
