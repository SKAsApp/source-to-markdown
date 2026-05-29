// Copilot作成
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.GitIgnore;

/// <summary>
/// 既定除外と簡易gitignoreルールによる除外判定を行います。
/// </summary>
public sealed class GitIgnoreRuleEvaluator : IIgnoreRuleEvaluator
{
	private static readonly HashSet<string> DefaultExcludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", "lib", "node_modules", "dist", "build", ".git" };

	/// <summary>
	/// 指定された相対パスが除外対象かどうかを判定します。
	/// </summary>
	public IgnoreDecision Evaluate(string sourceDirectoryPath, string relativePath)
	{
		string[] segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Any(segment => DefaultExcludedDirectories.Contains(segment)))
		{
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedDefaultExcludedDirectory, Reason = "既定除外ディレクトリーに該当します。" };
		}

		string gitIgnorePath = Path.Combine(sourceDirectoryPath, ".gitignore");
		if (File.Exists(gitIgnorePath) && IsMatchedBySimpleGitIgnore(gitIgnorePath, relativePath))
		{
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedByIgnoreRule, Reason = ".gitignoreに該当します。" };
		}

		return new IgnoreDecision { IsIgnored = false };
	}

	/// <summary>
	/// ルートのgitignoreに対して基本的なパターン一致を行います。
	/// </summary>
	private static bool IsMatchedBySimpleGitIgnore(string gitIgnorePath, string relativePath)
	{
		foreach (string rawLine in File.ReadLines(gitIgnorePath))
		{
			string line = rawLine.Trim();
			if (line.Length == 0 || line.StartsWith('#') || line.StartsWith('!'))
			{
				continue;
			}
			string pattern = line.TrimStart('/');
			if (pattern.EndsWith('/'))
			{
				string directoryPattern = pattern.TrimEnd('/');
				if (relativePath.Split('/').Contains(directoryPattern, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			else if (relativePath.Equals(pattern, StringComparison.OrdinalIgnoreCase) || relativePath.EndsWith("/" + pattern, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
