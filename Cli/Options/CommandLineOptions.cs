// Copilot作成
namespace SourceToMarkdown.Cli.Options;

/// <summary>
/// コマンドライン引数から生成される実行オプションを表します。
/// </summary>
public sealed class CommandLineOptions
{
	public string SourceDirectoryPath { get; set; } = string.Empty;
	public string OutputMarkdownPath { get; set; } = string.Empty;
	public string? LanguageMapPath { get; set; }
	public bool Verbose { get; set; }
	public bool Force { get; set; }
	public bool ShowHelp { get; set; }
}
