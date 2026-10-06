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
	/// <param name="args">コマンドライン引数</param>
	/// <returns>実行オプション</returns>
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
				continue;
			}
			if (argument == "--verbose")
			{
				options.Verbose = true;
				continue;
			}
			if (argument == "--force")
			{
				options.Force = true;
				continue;
			}
			if (argument == "--reverse")
			{
				options.Reverse = true;
				continue;
			}
			if (argument == "--language-map")
			{
				if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
				{
					options.ParseErrorMessage = "--language-mapの後にファイルパスを指定してください。";
					return options;
				}
				index++;
				options.LanguageMapPath = args[index];
				continue;
			}
			if (argument.StartsWith("--", StringComparison.Ordinal))
			{
				options.ParseErrorMessage = $"未定義のオプションです: {argument}";
				return options;
			}
			positionalArguments.Add(argument);
		}
		if (positionalArguments.Count != 2 && !options.ShowHelp)
		{
			options.ParseErrorMessage = "入力パスと出力パスを1つずつ指定してください。";
			return options;
		}
		if (positionalArguments.Count == 2)
		{
			options.InputPath = positionalArguments[0];
			options.OutputPath = positionalArguments[1];
		}
		if (options.Reverse && !string.IsNullOrWhiteSpace(options.LanguageMapPath))
		{
			options.ParseErrorMessage = "逆モードでは--language-mapを指定できません。";
		}
		return options;
	}
}
