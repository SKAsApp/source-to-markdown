// Copilot作成
namespace SourceToMarkdown.Core.Enums;

/// <summary>
/// アプリケーションの終了コードを表します。
/// </summary>
public enum ExitCode
{
	Success = 0,
	ArgumentError = 1,
	RuntimeError = 2,
	PartialSkipped = 3
}
