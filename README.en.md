# Source to Markdown – A Tool for Aggregating  Source Code into A Markdown File for AI Chatbots

[日本語版](./README.md)

`source-to-markdown` is a .NET console application that aggregates source code and configuration files from a specified directory into Markdown files, making them easier to pass to chat-based, web UI-driven generative AI tools such as Microsoft 365 Copilot.

It eliminates the hassle of uploading multiple files individually and can be used when you wish to attach the entire project’s code, including its directory structure.


## Features

- Recursively scans the contents of a specified directory
- Consolidates target files into a single file as Markdown code blocks
- Supports exclusion based on `.gitignore`
- Supports default exclusion directories
    - `bin`
    - `obj`
    - `lib`
    - `node_modules`
    - `dist`
    - `build`
    - `.git`
- Excludes hidden files and directories
- Does not follow symbolic links
- Does not include the output Markdown file itself
- Manages file extension and language hints via a JSON file
- Supports reading UTF-8, UTF-8 with BOM, Shift-JIS and UTF-16 LE
- Normalises line breaks to LF
- Avoids code fence conflicts (when ``` is present in source code, changes the start of the code block to ~~~)
- Logs output to a file
- Supports the same code on macOS, Linux and Windows


## System Requirements

- OS: macOS, Linux, Windows
- .NET SDK 10 (if building)


## Installation

### Option 1: Pre-built binaries (recommended)

Download the file for your operating system from [Release](https://github.com/SKAsApp/source-to-markdown/releases) and extract it to a location of your choice.

### Option2: Building from source

Clone the repository.

```sh
git clone https://github.com/SKAsApp/source-to-markdown.git
cd ./source-to-markdown
```

Build the project.

For macOS Apple Silicon:

```sh
dotnet publish "./source-to-markdown.csproj" --configuration "Release" --runtime "osx-arm64"
```

For Linux ARM64:

```sh
dotnet publish "./source-to-markdown.csproj" --configuration "Release" --runtime "linux-arm64"
```

For Windows AMD64:

```powershell
dotnet.exe publish ".\source-to-markdown.csproj" --configuration "Release" --runtime "win-x64"
```


## Usage

For macOS and Linux:

```sh
source-to-markdown <source-directory> <output-markdown> [options]
```

For Windows:

```powershell
# Prevent garbled characters in console output
[Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding("utf-8")
# Run
source-to-markdown.exe <source-directory> <output-markdown> [options]
```


## Options

| Option | Description |
| --- | --- |
| `--language-map <path>` | Specifies a JSON file mapping file extensions to language hints. <br>* Files with extensions not defined in the JSON will not be exported to Markdown |
| `--verbose` | Displays the files being processed, reasons for skipping, the character encoding used, and the language hints applied. |
| `--force` | Overwrites existing Markdown output files without prompting. |
| `--help` | Displays help. |


### Example Command

```sh
source-to-markdown ./src ./output.md --force
```

An example of consolidating the entire current directory into `project.md`.

```sh
source-to-markdown . ./project.md --force
```


## Format of the Output Markdown

The output Markdown will be in the following format.

~~~markdown
# ./src

`Program.cs`

```csharp
Console.WriteLine(‘Hello’);
```

---

`settings/appsettings.json`

```json
{
  "example": true
}
```
~~~

`---` is output as a separator between files.


## Language Hint Configuration

The mapping between file extensions and language hints is managed in `appsettings.language-map.json`.

Example:

```json
[
	{
		"extension": "cs",
		"fileType": "csharp"
	},
	{
		"extension": "csproj",
		"fileType": "xml"
	},
	{
		"extension": "json",
		"fileType": "json"
	},
	{
		"extension": "md",
		"fileType": "markdown"
	}
]
```

If you wish to use your own language hint JSON file, specify the `--language-map` option.

```sh
source-to-markdown ./src ./output.md --language-map ./my-language-map.json
```

## Logging Configuration

Serilog is used for logging.
By default, logs with a level of `Information` or higher are written to a file.

The configuration file is `appsettings.json`.

Example:

```json
{
	"logging":
	{
		"logDirectoryPath": null,
		"minimumLevel": "Information",
		"retainedFileCountLimit": 31
	}
}

```

### Configuration Options

| Option | Description |
| --- | --- |
| `logging.logDirectoryPath` | The destination directory for log files. If set to `null`, a `log` directory will be created in the directory where the executable file is located. |
| `logging.minimumLevel` | The minimum log level. The default is `Information`. |
| `logging.retainedFileCountLimit` | The number of log files to retain. The default is `31`. |

Log file names follow a daily rolling format.

```text
source-to-markdown-YYYYMMDD.log
```


## Exclusion Rules

The following items are excluded from output:

- Files or directories matching `.gitignore`
- Directories under the default exclusion directory
- Hidden files
- Directories under hidden directories
- Symbolic links
- The output Markdown file itself
- Files with unknown extensions (those not defined in the JSON file)
- Files with an unrecognised character encoding
- Files identified as binary
- Files larger than 50 MB


## Exit Codes

| Exit Code | Meaning |
| --- | --- |
| `0` | Success |
| `1` | Argument error |
| `2` | Runtime error |
| `3` | Partial skip or warnings |


## Testing

Automated tests are located in `Tests/source-to-markdown.Tests`.

```sh
dotnet test ./Tests/source-to-markdown.Tests/source-to-markdown.Tests.csproj
```


## Directory Structure and Guidelines

- Single application project structure
- Tests are managed as a separate project under `Tests`
- Does not rely on Visual Studio-specific features
- Folders and namespaces are separated by responsibility
    - `Cli`
    - `Core`
    - `Infrastructure`
    - `Tests`

```text
source-to-markdown
├── source-to-markdown.csproj
├── Program.cs
├── appsettings.json
├── appsettings.language-map.json
├── Cli
│   ├── Options
│   └── Output
├── Core
│   ├── Enums
│   ├── Interfaces
│   ├── Models
│   └── UseCases
├── Infrastructure
│   ├── Encoding
│   ├── FileSystem
│   ├── GitIgnore
│   ├── Logging
│   └── Markdown
└── Tests
    └── source-to-markdown.Tests
```


## Important Notes

- If the source directory contains any sensitive information, please ensure you check the contents before sharing the output Markdown.
- This tool does not analyse source code or mask confidential information.


## Licence

The licence is GPL-3.0 (GNU General Public License Version 3).

---

Translated with DeepL.com
