// Copilot作成
using System.Text.RegularExpressions;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.GitIgnore;

/// <summary>
/// 既定除外とgitignore互換寄りのルールによる除外判定を行います。
/// </summary>
public sealed class GitIgnoreRuleEvaluator : IIgnoreRuleEvaluator
{
	private static readonly HashSet<string> DefaultExcludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", "lib", "node_modules", "dist", "build", ".git" };

	/// <summary>
	/// 指定された相対パスが除外対象かどうかを判定します。
	/// </summary>
	public IgnoreDecision Evaluate(string sourceDirectoryPath, string relativePath)
	{
		string normalizedRelativePath = NormalizePath(relativePath);
		string[] segments = normalizedRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Any(segment => DefaultExcludedDirectories.Contains(segment)))
		{
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedDefaultExcludedDirectory, Reason = "既定除外ディレクトリーに該当します。" };
		}

		IReadOnlyList<GitIgnoreRule> rules = LoadRules(sourceDirectoryPath, normalizedRelativePath);
		GitIgnoreRule? matchedRule = null;
		bool isIgnored = false;
		foreach (GitIgnoreRule rule in rules)
		{
			if (IsMatch(rule, normalizedRelativePath))
			{
				matchedRule = rule;
				isIgnored = !rule.IsNegated;
			}
		}

		if (isIgnored && matchedRule != null)
		{
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedByIgnoreRule, Reason = $".gitignoreに該当します。パターン: {matchedRule.DisplayPattern}" };
		}

		return new IgnoreDecision { IsIgnored = false };
	}

	/// <summary>
	/// 対象ファイルに適用可能なgitignoreルールを読み込みます。
	/// </summary>
	private static IReadOnlyList<GitIgnoreRule> LoadRules(string sourceDirectoryPath, string relativePath)
	{
		List<GitIgnoreRule> rules = new List<GitIgnoreRule>();
		string fullSourceDirectoryPath = Path.GetFullPath(sourceDirectoryPath);
		List<string> relativeDirectories = GetRuleDirectoryPaths(relativePath);
		foreach (string relativeDirectory in relativeDirectories)
		{
			string gitIgnorePath = Path.Combine(fullSourceDirectoryPath, relativeDirectory.Replace('/', Path.DirectorySeparatorChar), ".gitignore");
			if (!File.Exists(gitIgnorePath))
			{
				continue;
			}

			foreach (GitIgnoreRule rule in ParseRules(gitIgnorePath, NormalizePath(relativeDirectory)))
			{
				rules.Add(rule);
			}
		}

		return rules;
	}

	/// <summary>
	/// 対象ファイルまでの各階層にあるgitignore配置ディレクトリーを取得します。
	/// </summary>
	private static List<string> GetRuleDirectoryPaths(string relativePath)
	{
		List<string> directories = new List<string> { string.Empty };
		string? directoryName = Path.GetDirectoryName(relativePath.Replace('/', Path.DirectorySeparatorChar));
		if (string.IsNullOrWhiteSpace(directoryName))
		{
			return directories;
		}

		string normalizedDirectoryName = NormalizePath(directoryName);
		string[] segments = normalizedDirectoryName.Split('/', StringSplitOptions.RemoveEmptyEntries);
		string currentPath = string.Empty;
		foreach (string segment in segments)
		{
			currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";
			directories.Add(currentPath);
		}

		return directories;
	}

	/// <summary>
	/// gitignoreファイルを解析してルール一覧を作成します。
	/// </summary>
	private static IEnumerable<GitIgnoreRule> ParseRules(string gitIgnorePath, string ruleBaseDirectory)
	{
		foreach (string rawLine in File.ReadLines(gitIgnorePath))
		{
			string? pattern = NormalizeRuleLine(rawLine);
			if (pattern == null)
			{
				continue;
			}

			bool isNegated = pattern.StartsWith("!", StringComparison.Ordinal);
			if (isNegated)
			{
				pattern = pattern[1..];
			}

			bool directoryOnly = pattern.EndsWith("/", StringComparison.Ordinal);
			pattern = pattern.Trim('/');
			if (string.IsNullOrWhiteSpace(pattern))
			{
				continue;
			}

			yield return new GitIgnoreRule(ruleBaseDirectory, NormalizePath(pattern), rawLine.Trim(), isNegated, directoryOnly, pattern.Contains("/", StringComparison.Ordinal));
		}
	}

	/// <summary>
	/// gitignoreの1行を判定用パターンへ正規化します。
	/// </summary>
	private static string? NormalizeRuleLine(string rawLine)
	{
		string line = rawLine.Trim();
		if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		{
			return null;
		}

		if (line.StartsWith(@"\#", StringComparison.Ordinal))
		{
			line = line[1..];
		}

		return line;
	}

	/// <summary>
	/// ルールが相対パスに一致するかどうかを判定します。
	/// </summary>
	private static bool IsMatch(GitIgnoreRule rule, string relativePath)
	{
		string pathToEvaluate = relativePath;
		if (!string.IsNullOrEmpty(rule.BaseDirectory))
		{
			if (!relativePath.Equals(rule.BaseDirectory, StringComparison.Ordinal) && !relativePath.StartsWith(rule.BaseDirectory + "/", StringComparison.Ordinal))
			{
				return false;
			}
			pathToEvaluate = relativePath.Length == rule.BaseDirectory.Length ? string.Empty : relativePath[(rule.BaseDirectory.Length + 1)..];
		}

		if (rule.DirectoryOnly)
		{
			return IsDirectoryPatternMatch(rule, pathToEvaluate);
		}

		return IsFilePatternMatch(rule, pathToEvaluate);
	}

	/// <summary>
	/// ディレクトリー専用パターンが一致するかどうかを判定します。
	/// </summary>
	private static bool IsDirectoryPatternMatch(GitIgnoreRule rule, string pathToEvaluate)
	{
		if (rule.ContainsSlash)
		{
			return IsGlobMatch(pathToEvaluate, rule.Pattern) || pathToEvaluate.StartsWith(rule.Pattern + "/", StringComparison.Ordinal);
		}

		return pathToEvaluate.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => IsGlobMatch(segment, rule.Pattern));
	}

	/// <summary>
	/// ファイルパターンが一致するかどうかを判定します。
	/// </summary>
	private static bool IsFilePatternMatch(GitIgnoreRule rule, string pathToEvaluate)
	{
		if (rule.ContainsSlash)
		{
			return IsGlobMatch(pathToEvaluate, rule.Pattern);
		}

		string fileName = Path.GetFileName(pathToEvaluate.Replace('/', Path.DirectorySeparatorChar));
		return IsGlobMatch(fileName, rule.Pattern) || IsGlobMatch(pathToEvaluate, rule.Pattern);
	}

	/// <summary>
	/// 簡易globパターンが対象文字列に一致するかどうかを判定します。
	/// </summary>
	private static bool IsGlobMatch(string value, string pattern)
	{
		string regexPattern = "^" + Regex.Escape(pattern)
			.Replace(@"\*\*", ".*")
			.Replace(@"\*", "[^/]*")
			.Replace(@"\?", "[^/]") + "$";
		return Regex.IsMatch(value, regexPattern, RegexOptions.CultureInvariant);
	}

	/// <summary>
	/// パス区切りをスラッシュへ正規化します。
	/// </summary>
	private static string NormalizePath(string path)
	{
		return path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/').Trim('/');
	}

	/// <summary>
	/// gitignoreの1ルールを表します。
	/// </summary>
	private sealed class GitIgnoreRule
	{
		/// <summary>
		/// gitignoreが配置された相対ディレクトリーを取得します。
		/// </summary>
		public string BaseDirectory { get; }

		/// <summary>
		/// 正規化済みパターンを取得します。
		/// </summary>
		public string Pattern { get; }

		/// <summary>
		/// 表示用パターンを取得します。
		/// </summary>
		public string DisplayPattern { get; }

		/// <summary>
		/// 否定パターンかどうかを取得します。
		/// </summary>
		public bool IsNegated { get; }

		/// <summary>
		/// ディレクトリー専用パターンかどうかを取得します。
		/// </summary>
		public bool DirectoryOnly { get; }

		/// <summary>
		/// パターンにパス区切りが含まれるかどうかを取得します。
		/// </summary>
		public bool ContainsSlash { get; }

		/// <summary>
		/// gitignoreルールを初期化します。
		/// </summary>
		public GitIgnoreRule(string baseDirectory, string pattern, string displayPattern, bool isNegated, bool directoryOnly, bool containsSlash)
		{
			this.BaseDirectory = baseDirectory;
			this.Pattern = pattern;
			this.DisplayPattern = displayPattern;
			this.IsNegated = isNegated;
			this.DirectoryOnly = directoryOnly;
			this.ContainsSlash = containsSlash;
		}
	}
}
