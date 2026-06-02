// Copilot作成
using Serilog;
using Serilog.Events;
using SourceToMarkdown.Core.Models;

namespace SourceToMarkdown.Infrastructure.Logging;

/// <summary>
/// Serilogのログ出力設定を構成します。
/// </summary>
public sealed class SerilogConfigurator
{
	/// <summary>
	/// 指定された設定に従ってSerilogのグローバルロガーを初期化します。
	/// </summary>
	public void Configure(LoggingSettings settings)
	{
		string logDirectoryPath = this.ResolveLogDirectoryPath(settings.LogDirectoryPath);
		Directory.CreateDirectory(logDirectoryPath);
		string logFilePath = Path.Combine(logDirectoryPath, "source-to-markdown-.log");
		LogEventLevel minimumLevel = this.ResolveMinimumLevel(settings.MinimumLevel);
		int retainedFileCountLimit = settings.RetainedFileCountLimit <= 0 ? 31 : settings.RetainedFileCountLimit;

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Is(minimumLevel)
			.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
			.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: retainedFileCountLimit, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
			.CreateLogger();
	}

	/// <summary>
	/// ログディレクトリーの実パスを決定します。
	/// </summary>
	private string ResolveLogDirectoryPath(string? configuredLogDirectoryPath)
	{
		if (!string.IsNullOrWhiteSpace(configuredLogDirectoryPath))
		{
			return Path.GetFullPath(configuredLogDirectoryPath);
		}

		return Path.Combine(AppContext.BaseDirectory, "log");
	}

	/// <summary>
	/// 設定文字列からSerilogのログレベルへ変換します。
	/// </summary>
	private LogEventLevel ResolveMinimumLevel(string? minimumLevel)
	{
		if (Enum.TryParse(minimumLevel, true, out LogEventLevel parsedLevel))
		{
			return parsedLevel;
		}

		return LogEventLevel.Information;
	}
}
