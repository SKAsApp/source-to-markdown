// Copilot作成
using SourceToMarkdown.Core.Enums;

namespace SourceToMarkdown.Core.UseCases;

/// <summary>
/// 処理状況から終了コードを決定します。
/// </summary>
public sealed class ExitCodeResolver
{
	/// <summary>
	/// 警告有無に応じて終了コードを決定します。
	/// </summary>
	public ExitCode Resolve(bool hasWarnings)
	{
		return hasWarnings ? ExitCode.PartialSkipped : ExitCode.Success;
	}
}
