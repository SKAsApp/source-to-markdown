// Copilot作成
namespace SourceToMarkdown.Cli.Options;

/// <summary>
/// コマンドライン引数を解析します。
/// </summary>
public sealed class CommandLineParser
{
	/// <summary>
	/// コマンドライン引数を解析して実行オプションを作成します。
	/// </summary>
	public CommandLineOptions Parse(string[] args)
	{
		CommandLineOptions options = new CommandLineOptions();
		List<string> positionalArguments = new List<string>();

		for (int index = 0; index < args.Length; index++)
		{
			string argument = args[index];
			if (argument == "--help" || argument == "-h")
			{
				options.ShowHelp = true;
			}
			else if (argument == "--verbose")
			{
				options.Verbose = true;
			}
			else if (argument == "--force")
			{
				options.Force = true;
			}
			else if (argument == "--language-map")
			{
				if (index + 1 < args.Length)
				{
					index++;
					options.LanguageMapPath = args[index];
				}
			}
			else
			{
				positionalArguments.Add(argument);
			}
		}

		if (positionalArguments.Count > 0)
		{
			options.SourceDirectoryPath = positionalArguments[0];
		}
		if (positionalArguments.Count > 1)
		{
			options.OutputMarkdownPath = positionalArguments[1];
		}

		return options;
	}
}
