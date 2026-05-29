// Copilot作成
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Core.Interfaces;

/// <summary>
/// ファイルが読み取り可能なテキストか判定する機能を表します。
/// </summary>
public interface ITextFileDetector
{
	/// <summary>
	/// 指定されたファイルのテキスト判定を行います。
	/// </summary>
	TextDetectionResult Detect(string filePath, long maxFileSizeBytes);
}
