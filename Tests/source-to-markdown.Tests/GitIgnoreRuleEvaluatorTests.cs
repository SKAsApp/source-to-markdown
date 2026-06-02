// Copilot作成
using SourceToMarkdown.Infrastructure.GitIgnore;

namespace SourceToMarkdown.Tests;

/// <summary>
/// GitIgnoreRuleEvaluatorのテストを提供します。
/// </summary>
public sealed class GitIgnoreRuleEvaluatorTests
{
	/// <summary>
	/// .gitignoreに一致するファイルが除外されることを確認します。
	/// </summary>
	[Fact]
	public void Evaluate_ReturnsIgnoredForGitIgnoreMatchedPath()
	{
		string rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		try
		{
			Directory.CreateDirectory(rootPath);
			File.WriteAllText(Path.Combine(rootPath, ".gitignore"), "*.secret" + Environment.NewLine);
			GitIgnoreRuleEvaluator evaluator = new GitIgnoreRuleEvaluator();
			var decision = evaluator.Evaluate(rootPath, "sample.secret");
			Assert.True(decision.IsIgnored);
		}
		finally
		{
			if (Directory.Exists(rootPath))
			{
				Directory.Delete(rootPath, true);
			}
		}
	}
}
