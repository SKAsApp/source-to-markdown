// Copilot作成
using System.Text.Json;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Core.UseCases;

/// <summary>
/// アプリケーション設定を読み込みます。
/// </summary>
public sealed class AppSettingsLoader
{
	/// <summary>
	/// 言語ヒントJSONを読み込みます。
	/// </summary>
	public IReadOnlyList<LanguageHintEntry> LoadLanguageHints(string? languageMapPath)
	{
		string path = this.ResolveLanguageMapPath(languageMapPath);
		string json = File.ReadAllText(path);
		List<LanguageHintEntry>? entries = JsonSerializer.Deserialize<List<LanguageHintEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		return entries is null ? Array.Empty<LanguageHintEntry>() : entries;
	}

	/// <summary>
	/// 言語ヒントJSONの読み込みパスを決定します。
	/// </summary>
	private string ResolveLanguageMapPath(string? languageMapPath)
	{
		if (!string.IsNullOrWhiteSpace(languageMapPath))
		{
			return Path.GetFullPath(languageMapPath);
		}

		string baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, "appsettings.language-map.json");
		if (File.Exists(baseDirectoryPath))
		{
			return baseDirectoryPath;
		}

		string currentDirectoryPath = Path.Combine(Environment.CurrentDirectory, "appsettings.language-map.json");
		if (File.Exists(currentDirectoryPath))
		{
			return currentDirectoryPath;
		}

		return "appsettings.language-map.json";
	}
}
