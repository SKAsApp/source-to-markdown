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
	/// アプリケーション設定JSONを読み込みます。
	/// </summary>
	public ApplicationSettings LoadApplicationSettings()
	{
		string path = this.ResolveApplicationSettingsPath();
		if (!File.Exists(path))
		{
			return new ApplicationSettings();
		}

		string json = File.ReadAllText(path);
		ApplicationSettings? settings = JsonSerializer.Deserialize<ApplicationSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		return settings ?? new ApplicationSettings();
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

		return this.ResolveExistingOrDefaultPath("appsettings.language-map.json");
	}

	/// <summary>
	/// アプリケーション設定JSONの読み込みパスを決定します。
	/// </summary>
	private string ResolveApplicationSettingsPath()
	{
		return this.ResolveExistingOrDefaultPath("appsettings.json");
	}

	/// <summary>
	/// 実行ファイル配置先またはカレントディレクトリーから設定ファイルのパスを決定します。
	/// </summary>
	private string ResolveExistingOrDefaultPath(string fileName)
	{
		string baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, fileName);
		if (File.Exists(baseDirectoryPath))
		{
			return baseDirectoryPath;
		}

		string currentDirectoryPath = Path.Combine(Environment.CurrentDirectory, fileName);
		if (File.Exists(currentDirectoryPath))
		{
			return currentDirectoryPath;
		}

		return fileName;
	}
}
