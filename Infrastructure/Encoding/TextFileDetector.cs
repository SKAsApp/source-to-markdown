// Copilot作成
using System.Text;
using SourceToMarkdown.Core.Enums;
using SourceToMarkdown.Core.Interfaces;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.Encoding;

/// <summary>
/// ファイルのサイズ、バイナリー性、文字コードを判定します。
/// </summary>
public sealed class TextFileDetector : ITextFileDetector
{
	/// <summary>
	/// 指定されたファイルのテキスト判定を行います。
	/// </summary>
	public TextDetectionResult Detect(string filePath, long maxFileSizeBytes)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		if (fileInfo.Length > maxFileSizeBytes)
		{
			return new TextDetectionResult { IsText = false, WarningType = WarningType.SkippedTooLarge, Message = "ファイルサイズが上限を超えています。" };
		}

		byte[] bytes = File.ReadAllBytes(filePath);
		if (bytes.Contains((byte)0) && !LooksLikeUtf16Le(bytes))
		{
			return new TextDetectionResult { IsText = false, WarningType = WarningType.SkippedEncodingUnknown, Message = "バイナリーファイルの可能性があります。" };
		}

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		Encoding? encoding = DetectEncoding(bytes);
		if (encoding == null)
		{
			return new TextDetectionResult { IsText = false, WarningType = WarningType.SkippedEncodingUnknown, Message = "文字コードを判定できません。" };
		}
		return new TextDetectionResult { IsText = true, Encoding = encoding };
	}

	/// <summary>
	/// バイト列から対応文字コードを推定します。
	/// </summary>
	private static Encoding? DetectEncoding(byte[] bytes)
	{
		if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
		{
			return new UTF8Encoding(true, true);
		}
		if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
		{
			return Encoding.Unicode;
		}
		if (CanDecode(bytes, new UTF8Encoding(false, true)))
		{
			return new UTF8Encoding(false, true);
		}
		Encoding shiftJis = Encoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
		if (CanDecode(bytes, shiftJis))
		{
			return shiftJis;
		}
		if (LooksLikeUtf16Le(bytes) && CanDecode(bytes, Encoding.Unicode))
		{
			return Encoding.Unicode;
		}
		return null;
	}

	/// <summary>
	/// 指定された文字コードでデコード可能かどうかを判定します。
	/// </summary>
	private static bool CanDecode(byte[] bytes, Encoding encoding)
	{
		try
		{
			_ = encoding.GetString(bytes);
			return true;
		}
		catch (DecoderFallbackException)
		{
			return false;
		}
	}

	/// <summary>
	/// バイト列がUTF-16 LEらしい構造かどうかを判定します。
	/// </summary>
	private static bool LooksLikeUtf16Le(byte[] bytes)
	{
		if (bytes.Length < 4 || bytes.Length % 2 != 0)
		{
			return false;
		}
		int zeroOddCount = 0;
		for (int index = 1; index < bytes.Length; index += 2)
		{
			if (bytes[index] == 0)
			{
				zeroOddCount++;
			}
		}
		return zeroOddCount > bytes.Length / 4;
	}
}
