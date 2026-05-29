// Copilot作成
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// 除外ルールを評価する機能を表します。
/// </summary>
public interface IIgnoreRuleEvaluator
{
	/// <summary>
	/// 指定された相対パスが除外対象かどうかを判定します。
	/// </summary>
	IgnoreDecision Evaluate(string sourceDirectoryPath, string relativePath);
}
