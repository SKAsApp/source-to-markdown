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
		string path = string.IsNullOrWhiteSpace(languageMapPath) ? Path.Combine(AppContext.BaseDirectory, "appsettings.language-map.json") : languageMapPath;
		if (!File.Exists(path))
		{
			path = "appsettings.language-map.json";
		}

		string json = File.ReadAllText(path);
		List<LanguageHintEntry>? entries = JsonSerializer.Deserialize<List<LanguageHintEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		return entries is null ? Array.Empty<LanguageHintEntry>() : entries;
	}
}
