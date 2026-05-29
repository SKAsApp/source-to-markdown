// Copilot作成
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// ファイル候補を収集する機能を表します。
/// </summary>
public interface IFileCollector
{
	/// <summary>
	/// 指定された入力ディレクトリーから候補ファイルを収集します。
	/// </summary>
	IEnumerable<FileCandidate> Collect(string sourceDirectoryPath, string outputMarkdownPath);
}
