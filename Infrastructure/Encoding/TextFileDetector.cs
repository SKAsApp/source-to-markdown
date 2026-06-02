// Copilot作成
using System.Text;
using TextEncoding = System.Text.Encoding;
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
		if (bytes.Length == 0)
		{
			return new TextDetectionResult { IsText = true, Encoding = new UTF8Encoding(false, true), Message = "空ファイルです。" };
		}

		TextEncoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		TextEncoding? encoding = DetectEncoding(bytes);
		if (encoding == null)
		{
			return new TextDetectionResult { IsText = false, WarningType = WarningType.SkippedEncodingUnknown, Message = "文字コードを判定できません。" };
		}

		if (ContainsBinaryNull(bytes, encoding))
		{
			return new TextDetectionResult { IsText = false, WarningType = WarningType.SkippedEncodingUnknown, Message = "バイナリーファイルの可能性があります。" };
		}

		return new TextDetectionResult { IsText = true, Encoding = encoding };
	}

	/// <summary>
	/// バイト列から対応文字コードを推定します。
	/// </summary>
	private static TextEncoding? DetectEncoding(byte[] bytes)
	{
		if (HasUtf8Bom(bytes))
		{
			return new UTF8Encoding(true, true);
		}

		if (HasUtf16LeBom(bytes))
		{
			return TextEncoding.Unicode;
		}

		if (CanDecode(bytes, new UTF8Encoding(false, true)))
		{
			return new UTF8Encoding(false, true);
		}

		TextEncoding shiftJis = TextEncoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
		if (CanDecode(bytes, shiftJis))
		{
			return shiftJis;
		}

		if (LooksLikeUtf16Le(bytes) && CanDecode(bytes, TextEncoding.Unicode))
		{
			return TextEncoding.Unicode;
		}

		return null;
	}

	/// <summary>
	/// UTF-8のBOMがあるかどうかを判定します。
	/// </summary>
	private static bool HasUtf8Bom(byte[] bytes)
	{
		return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
	}

	/// <summary>
	/// UTF-16 LEのBOMがあるかどうかを判定します。
	/// </summary>
	private static bool HasUtf16LeBom(byte[] bytes)
	{
		return bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE;
	}

	/// <summary>
	/// 指定された文字コードでデコード可能かどうかを判定します。
	/// </summary>
	private static bool CanDecode(byte[] bytes, TextEncoding encoding)
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
		int sampleCount = Math.Min(bytes.Length, 8192);
		for (int index = 1; index < sampleCount; index += 2)
		{
			if (bytes[index] == 0)
			{
				zeroOddCount++;
			}
		}

		return zeroOddCount > sampleCount / 4;
	}

	/// <summary>
	/// バイナリーを疑うNULLバイトが含まれるかどうかを判定します。
	/// </summary>
	private static bool ContainsBinaryNull(byte[] bytes, TextEncoding encoding)
	{
		if (encoding.CodePage == TextEncoding.Unicode.CodePage)
		{
			return false;
		}

		int sampleCount = Math.Min(bytes.Length, 8192);
		for (int index = 0; index < sampleCount; index++)
		{
			if (bytes[index] == 0)
			{
				return true;
			}
		}

		return false;
	}
}
