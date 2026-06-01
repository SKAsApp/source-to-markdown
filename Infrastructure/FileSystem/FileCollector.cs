// Copilot作成
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.FileSystem;

/// <summary>
/// ファイルシステムから候補ファイルを収集します。
/// </summary>
public sealed class FileCollector : IFileCollector
{
	/// <summary>
	/// 指定された入力ディレクトリーから候補ファイルを収集します。
	/// </summary>
	public IEnumerable<FileCandidate> Collect(string sourceDirectoryPath, string outputMarkdownPath)
	{
		string fullSourcePath = Path.GetFullPath(sourceDirectoryPath);
		string fullOutputPath = Path.GetFullPath(outputMarkdownPath);
		List<FileCandidate> candidates = new List<FileCandidate>();
		CollectRecursive(fullSourcePath, fullSourcePath, fullOutputPath, candidates);
		return candidates.OrderBy(candidate => candidate.RelativePath, StringComparer.Ordinal).ToList();
	}

	/// <summary>
	/// ディレクトリーを再帰的に探索して候補ファイル一覧へ追加します。
	/// </summary>
	private static void CollectRecursive(string rootPath, string currentPath, string outputPath, List<FileCandidate> candidates)
	{
		foreach (string directoryPath in Directory.EnumerateDirectories(currentPath))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			if (IsHidden(directoryInfo) || IsSymbolicLink(directoryInfo))
			{
				continue;
			}
			CollectRecursive(rootPath, directoryPath, outputPath, candidates);
		}

		foreach (string filePath in Directory.EnumerateFiles(currentPath))
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (IsHidden(fileInfo) || IsSymbolicLink(fileInfo) || Path.GetFullPath(filePath) == outputPath)
			{
				continue;
			}
			string relativePath = Path.GetRelativePath(rootPath, filePath).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
			candidates.Add(new FileCandidate { FullPath = filePath, RelativePath = relativePath });
		}
	}

	/// <summary>
	/// ファイルシステム項目が隠し項目かどうかを判定します。
	/// </summary>
	private static bool IsHidden(FileSystemInfo fileSystemInfo)
	{
		return fileSystemInfo.Name.StartsWith(".", StringComparison.Ordinal) || fileSystemInfo.Attributes.HasFlag(FileAttributes.Hidden);
	}

	/// <summary>
	/// ファイルシステム項目がシンボリックリンクかどうかを判定します。
	/// </summary>
	private static bool IsSymbolicLink(FileSystemInfo fileSystemInfo)
	{
		return fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
	}
}
