// Copilot作成
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.GitIgnore;

/// <summary>
/// 既定除外とGitignoreParserNetによる.gitignore判定を行います。
/// </summary>
public sealed class GitIgnoreRuleEvaluator : IIgnoreRuleEvaluator
{
	private static readonly HashSet<string> DefaultExcludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", "lib", "node_modules", "dist", "build", ".git" };
	private readonly GitignoreParserNetAdapter gitignoreParserNetAdapter = new GitignoreParserNetAdapter();

	/// <summary>
	/// 指定された相対パスが除外対象かどうかを判定します。
	/// </summary>
	public IgnoreDecision Evaluate(string sourceDirectoryPath, string relativePath)
	{
		string normalizedRelativePath = NormalizePath(relativePath);
		string[] segments = normalizedRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Any(segment => DefaultExcludedDirectories.Contains(segment)))
		{
			Log.Debug("既定除外ディレクトリーにより除外します。Path={RelativePath}", normalizedRelativePath);
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedDefaultExcludedDirectory, Reason = "既定除外ディレクトリーに該当します。" };
		}

		GitIgnoreMatchResult libraryResult = this.gitignoreParserNetAdapter.Evaluate(sourceDirectoryPath, normalizedRelativePath);
		if (libraryResult.IsMatched)
		{
			if (libraryResult.IsIgnored)
			{
				Log.Debug("GitignoreParserNetにより除外します。Path={RelativePath}", normalizedRelativePath);
				return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedByIgnoreRule, Reason = ".gitignoreに該当します。" };
			}

			return new IgnoreDecision { IsIgnored = false };
		}

		GitIgnoreMatchResult fallbackResult = EvaluateByFallback(sourceDirectoryPath, normalizedRelativePath);
		if (fallbackResult.IsIgnored)
		{
			Log.Debug("フォールバックgitignore判定により除外します。Path={RelativePath}", normalizedRelativePath);
			return new IgnoreDecision { IsIgnored = true, WarningType = WarningType.SkippedByIgnoreRule, Reason = $".gitignoreに該当します。パターン: {fallbackResult.Pattern}" };
		}

		return new IgnoreDecision { IsIgnored = false };
	}

	/// <summary>
	/// GitignoreParserNetで判定できなかった場合の補助判定を行います。
	/// </summary>
	private static GitIgnoreMatchResult EvaluateByFallback(string sourceDirectoryPath, string relativePath)
	{
		IReadOnlyList<GitIgnoreRule> rules = LoadRules(sourceDirectoryPath, relativePath);
		GitIgnoreRule? matchedRule = null;
		bool isIgnored = false;
		foreach (GitIgnoreRule rule in rules)
		{
			if (IsMatch(rule, relativePath))
			{
				matchedRule = rule;
				isIgnored = !rule.IsNegated;
			}
		}

		if (matchedRule == null)
		{
			return GitIgnoreMatchResult.NotMatched();
		}

		return new GitIgnoreMatchResult(true, isIgnored, matchedRule.DisplayPattern);
	}

	/// <summary>
	/// 対象ファイルに適用可能なgitignoreルールを読み込みます。
	/// </summary>
	private static IReadOnlyList<GitIgnoreRule> LoadRules(string sourceDirectoryPath, string relativePath)
	{
		List<GitIgnoreRule> rules = new List<GitIgnoreRule>();
		string fullSourceDirectoryPath = Path.GetFullPath(sourceDirectoryPath);
		foreach (string relativeDirectory in GetRuleDirectoryPaths(relativePath))
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
	/// GitignoreParserNetを反射で呼び出すアダプターを表します。
	/// </summary>
	private sealed class GitignoreParserNetAdapter
	{
		/// <summary>
		/// GitignoreParserNetを用いて相対パスを判定します。
		/// </summary>
		public GitIgnoreMatchResult Evaluate(string sourceDirectoryPath, string relativePath)
		{
			try
			{
				foreach (string gitIgnorePath in Directory.EnumerateFiles(sourceDirectoryPath, ".gitignore", SearchOption.AllDirectories))
				{
					string baseDirectory = NormalizePath(Path.GetRelativePath(sourceDirectoryPath, Path.GetDirectoryName(gitIgnorePath) ?? sourceDirectoryPath));
					string pathToEvaluate = GetPathToEvaluate(baseDirectory, relativePath);
					if (pathToEvaluate.Length == 0)
					{
						continue;
					}

					object? parser = this.CreateParser(File.ReadAllText(gitIgnorePath), gitIgnorePath);
					if (parser == null)
					{
						continue;
					}

					GitIgnoreMatchResult result = this.InvokeParser(parser, pathToEvaluate);
					if (result.IsMatched)
					{
						return result;
					}
				}
			}
			catch (Exception exception)
			{
				Log.Debug(exception, "GitignoreParserNetの呼び出しで例外が発生したためフォールバック判定へ移行します。");
			}

			return GitIgnoreMatchResult.NotMatched();
		}

		/// <summary>
		/// gitignore配置ディレクトリーから見た評価対象パスを取得します。
		/// </summary>
		private static string GetPathToEvaluate(string baseDirectory, string relativePath)
		{
			if (string.IsNullOrEmpty(baseDirectory))
			{
				return relativePath;
			}

			if (!relativePath.StartsWith(baseDirectory + "/", StringComparison.Ordinal))
			{
				return string.Empty;
			}

			return relativePath[(baseDirectory.Length + 1)..];
		}

		/// <summary>
		/// GitignoreParserNetのパーサーインスタンスを作成します。
		/// </summary>
		private object? CreateParser(string gitIgnoreContent, string gitIgnorePath)
		{
			Type? parserType = Type.GetType("GitignoreParserNet.GitignoreParser, GitignoreParserNet") ?? Type.GetType("GitignoreParser.GitignoreParser, GitignoreParserNet") ?? Type.GetType("GitignoreParser, GitignoreParserNet");
			if (parserType == null)
			{
				return null;
			}

			ConstructorInfo? contentConstructor = parserType.GetConstructor(new[] { typeof(string) });
			if (contentConstructor != null)
			{
				return contentConstructor.Invoke(new object[] { gitIgnoreContent });
			}

			MethodInfo? compileMethod = parserType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(method => method.Name == "Compile" && method.GetParameters().Length == 1);
			if (compileMethod != null)
			{
				return compileMethod.Invoke(null, new object[] { gitIgnoreContent });
			}

			MethodInfo? parseMethod = parserType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(method => method.Name == "Parse" && method.GetParameters().Any(parameter => parameter.Name == "gitignorePath"));
			if (parseMethod != null)
			{
				object?[] arguments = parseMethod.GetParameters().Select(parameter => parameter.ParameterType == typeof(bool) ? (object?)true : gitIgnorePath).ToArray();
				return parseMethod.Invoke(null, arguments);
			}

			return null;
		}

		/// <summary>
		/// GitignoreParserNetの判定メソッドを呼び出します。
		/// </summary>
		private GitIgnoreMatchResult InvokeParser(object parser, string pathToEvaluate)
		{
			Type parserType = parser.GetType();
			bool inspected = this.TryInvokeBoolean(parser, parserType, "Inspects", pathToEvaluate, out bool isInspected);
			bool denied = this.TryInvokeBoolean(parser, parserType, "Denies", pathToEvaluate, out bool isDenied);
			bool accepted = this.TryInvokeBoolean(parser, parserType, "Accepts", pathToEvaluate, out bool isAccepted);

			if (denied)
			{
				return new GitIgnoreMatchResult(true, isDenied, string.Empty);
			}

			if (inspected && isInspected)
			{
				return new GitIgnoreMatchResult(true, denied && isDenied, string.Empty);
			}

			if (accepted && !isAccepted)
			{
				return new GitIgnoreMatchResult(true, true, string.Empty);
			}

			return GitIgnoreMatchResult.NotMatched();
		}

		/// <summary>
		/// 指定された名前の真偽値戻りメソッドを呼び出します。
		/// </summary>
		private bool TryInvokeBoolean(object parser, Type parserType, string methodName, string pathToEvaluate, out bool value)
		{
			value = false;
			MethodInfo? method = parserType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(candidate => candidate.Name == methodName);
			if (method == null)
			{
				return false;
			}

			ParameterInfo[] parameters = method.GetParameters();
			object?[] arguments = parameters.Length switch
			{
				1 => new object?[] { pathToEvaluate },
				2 => new object?[] { pathToEvaluate, false },
				_ => Array.Empty<object?>()
			};
			if (arguments.Length == 0)
			{
				return false;
			}

			object? result = method.Invoke(parser, arguments);
			if (result is bool boolResult)
			{
				value = boolResult;
				return true;
			}

			return false;
		}
	}

	/// <summary>
	/// gitignore判定結果を表します。
	/// </summary>
	private sealed class GitIgnoreMatchResult
	{
		/// <summary>
		/// gitignoreルールに一致したかどうかを取得します。
		/// </summary>
		public bool IsMatched { get; }

		/// <summary>
		/// 除外対象かどうかを取得します。
		/// </summary>
		public bool IsIgnored { get; }

		/// <summary>
		/// 一致したパターンを取得します。
		/// </summary>
		public string Pattern { get; }

		/// <summary>
		/// gitignore判定結果を初期化します。
		/// </summary>
		public GitIgnoreMatchResult(bool isMatched, bool isIgnored, string pattern)
		{
			this.IsMatched = isMatched;
			this.IsIgnored = isIgnored;
			this.Pattern = pattern;
		}

		/// <summary>
		/// 未一致のgitignore判定結果を作成します。
		/// </summary>
		public static GitIgnoreMatchResult NotMatched()
		{
			return new GitIgnoreMatchResult(false, false, string.Empty);
		}
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
