// Copilot作成
using System.Text;
using SourceToMarkdown.Cli.Options;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;
namespace SourceToMarkdown.Core.UseCases;

/// <summary>
/// 本ツール形式のMarkdownからディレクトリーを復元します。
/// </summary>
public sealed class MarkdownToSourceUseCase
{
	/// <summary>Markdownを解析する機能</summary>
	private readonly IMarkdownRepositoryParser markdownRepositoryParser;

	/// <summary>
	/// 必要な依存機能を受け取り、ユースケースを初期化します。
	/// </summary>
	/// <param name="markdownRepositoryParser">Markdownを解析する機能</param>
	public MarkdownToSourceUseCase(IMarkdownRepositoryParser markdownRepositoryParser)
	{
		this.markdownRepositoryParser = markdownRepositoryParser;
	}

	/// <summary>
	/// Markdownからファイルを復元します。
	/// </summary>
	/// <param name="options">実行オプション</param>
	/// <returns>復元処理結果</returns>
	public MarkdownToSourceResult Execute(CommandLineOptions options)
	{
		IReadOnlyList<MarkdownFileBlock> blocks = this.markdownRepositoryParser.Parse(options.InputPath);
		string outputRootPath = Path.GetFullPath(options.OutputPath);
		List<(MarkdownFileBlock Block, string FullPath)> restorePlan = this.CreateRestorePlan(blocks, outputRootPath);
		List<string> existingFilePaths = restorePlan.Where(entry => File.Exists(entry.FullPath)).Select(entry => entry.FullPath).ToList();
		if (existingFilePaths.Count > 0 && !options.Force)
		{
			throw new InvalidOperationException($"復元先に既存ファイルが{existingFilePaths.Count}件あります。--forceを指定すると上書きできます。");
		}
		foreach ((MarkdownFileBlock Block, string FullPath) entry in restorePlan)
		{
			string? directoryPath = Path.GetDirectoryName(entry.FullPath);
			if (!string.IsNullOrWhiteSpace(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
			File.WriteAllText(entry.FullPath, entry.Block.Content, new UTF8Encoding(false));
			if (options.Verbose)
			{
				Console.Error.WriteLine($"詳細: 復元しました: {entry.Block.RelativePath}");
			}
		}
		return new MarkdownToSourceResult
		{
			RestoredFileCount = restorePlan.Count,
			ExitCode = ExitCode.Success
		};
	}

	/// <summary>
	/// 全ファイルの安全性を検証し、復元計画を作成します。
	/// </summary>
	/// <param name="blocks">解析済みファイルブロック</param>
	/// <param name="outputRootPath">復元先の絶対パス</param>
	/// <returns>検証済み復元計画</returns>
	private List<(MarkdownFileBlock Block, string FullPath)> CreateRestorePlan(IReadOnlyList<MarkdownFileBlock> blocks, string outputRootPath)
	{
		StringComparison pathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		StringComparer pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		string outputRootPrefix = outputRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
		HashSet<string> fullPaths = new HashSet<string>(pathComparer);
		List<(MarkdownFileBlock Block, string FullPath)> restorePlan = new List<(MarkdownFileBlock Block, string FullPath)>();
		foreach (MarkdownFileBlock block in blocks)
		{
			if (string.IsNullOrWhiteSpace(block.RelativePath) || Path.IsPathRooted(block.RelativePath))
			{
				throw new InvalidDataException($"不正な復元パスです: {block.RelativePath}");
			}
			string normalizedRelativePath = block.RelativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(outputRootPath, normalizedRelativePath));
			if (!fullPath.StartsWith(outputRootPrefix, pathComparison))
			{
				throw new InvalidDataException($"復元先の外部を指すパスです: {block.RelativePath}");
			}
			if (!fullPaths.Add(fullPath))
			{
				throw new InvalidDataException($"復元先が重複しています: {block.RelativePath}");
			}
			this.ValidateExistingParentPath(outputRootPath, fullPath);
			restorePlan.Add((block, fullPath));
		}
		this.ValidateFileDirectoryConflicts(fullPaths, pathComparer);
		return restorePlan;
	}

	/// <summary>
	/// 既存の親要素にシンボリックリンクがないことを確認します。
	/// </summary>
	/// <param name="outputRootPath">復元先の絶対パス</param>
	/// <param name="fullPath">復元ファイルの絶対パス</param>
	private void ValidateExistingParentPath(string outputRootPath, string fullPath)
	{
		string? currentPath = Path.GetDirectoryName(fullPath);
		while (!string.IsNullOrWhiteSpace(currentPath) && currentPath.Length >= outputRootPath.Length)
		{
			if (Directory.Exists(currentPath))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(currentPath);
				if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
				{
					throw new InvalidDataException($"シンボリックリンク配下には復元できません: {currentPath}");
				}
			}
			if (string.Equals(currentPath, outputRootPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return;
			}
			currentPath = Path.GetDirectoryName(currentPath);
		}
	}

	/// <summary>
	/// ファイルとして復元するパスと、その配下のパスが衝突しないことを確認します。
	/// </summary>
	/// <param name="fullPaths">復元対象の絶対パス一覧</param>
	/// <param name="pathComparer">パス比較規則</param>
	private void ValidateFileDirectoryConflicts(HashSet<string> fullPaths, StringComparer pathComparer)
	{
		foreach (string fullPath in fullPaths)
		{
			string? parentPath = Path.GetDirectoryName(fullPath);
			while (!string.IsNullOrWhiteSpace(parentPath))
			{
				if (fullPaths.Contains(parentPath))
				{
					throw new InvalidDataException($"ファイルとディレクトリーのパスが衝突しています: {fullPath}");
				}
				parentPath = Path.GetDirectoryName(parentPath);
			}
		}
	}
}
