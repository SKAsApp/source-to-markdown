# Source to Markdown

`source-to-markdown`は指定ディレクトリー配下のソースコードや設定ファイルを、Microsoft 365 CopilotなどのチャットWeb UIベースの生成AIへ渡しやすいMarkdownファイルへ集約するコンソールアプリです。

複数ファイルを個別にアップロードする手間を減らし、ディレクトリー構成を含めたプロジェクト全体のコードを添付したい場面で利用できます。


## 特徴

- 指定ディレクトリー配下を再帰的に探索
- 対象ファイルをMarkdownのコードブロックとして1ファイルへ集約
- `.gitignore`による除外判定に対応
- 既定除外ディレクトリーに対応
	- `bin`
	- `obj`
	- `lib`
	- `node_modules`
	- `dist`
	- `build`
	- `.git`
- 隠しファイルと隠しディレクトリーを除外
- シンボリックリンクを追跡しない
- 出力先Markdownファイル自身を取り込まない
- 拡張子と言語ヒントの対応をJSONファイルで管理
- UTF-8、UTF-8 BOM、Shift-JIS、UTF-16 LEの読み込みに対応
- 改行コードをLFに正規化
- コードフェンス衝突を回避（ソースコード内に```があるとき、~~~でのコードブロック開始に変更）
- ファイルへのログ出力
- macOS、Linux、Windowsの同一コード対応


## 動作環境

- OS：macOS、Linux、Windows
- .NET SDK 10 （ビルドする場合）


## インストール

### ビルド済みバイナリー（推奨）

[Release](https://github.com/SKAsApp/source-to-markdown/releases)から利用環境のOSのファイルをダウンロードし、任意の場所に展開します。

### 自前ビルドする場合

リポジトリーをクローンします。

```sh
git clone https://github.com/SKAsApp/source-to-markdown.git
cd ./source-to-markdown
```

ビルドします。

macOS Apple Siliconの場合：

```sh
dotnet publish "./source-to-markdown.csproj" --configuration "Release" --runtime "osx-arm64"
```

Linux ARM64の場合：

```sh
dotnet publish "./source-to-markdown.csproj" --configuration "Release" --runtime "linux-arm64"
```

Windows AMD64の場合：

```powershell
dotnet.exe publish "./source-to-markdown.csproj" --configuration "Release" --runtime "win-x64"
```


## 使い方

macOS、Linuxの場合：

```sh
source-to-markdown <source-directory> <output-markdown> [options]
```

Windowsの場合：

```powershell
# コンソール出力文字化け対策
[Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding("utf-8")
# 実行
source-to-markdown.exe <source-directory> <output-markdown> [options]
```


## オプション

| オプション | 説明 |
| --- | --- |
| `--language-map <path>` | 拡張子と言語ヒントの対応JSONファイルを指定します。<br>※ JSONに定義されていない拡張子のファイルはMarkdownに書き出しません |
| `--verbose` | 処理中ファイル、スキップ理由、採用文字コード、採用言語ヒントなどを表示します。 |
| `--force` | 既存の出力 Markdown ファイルを確認なしで上書きします。 |
| `--help` | ヘルプを表示します。 |


### 実行例

```sh
source-to-markdown ./src ./output.md --force
```

カレントディレクトリー全体を`project.md`に集約する例です。

```sh
source-to-markdown . ./project.md --force
```


## 出力 Markdown の形式

出力Markdownは以下の形式になります。

~~~markdown
# ./src

`Program.cs`

```csharp
Console.WriteLine("Hello");
```

---

`settings/appsettings.json`

```json
{
  "example": true
}
```
~~~

ファイル間の区切りには `---` を出力します。


## 言語ヒント設定

拡張子と言語ヒントの対応は`appsettings.language-map.json`で管理します。

例:

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

独自の言語ヒント JSON を使う場合は `--language-map` を指定します。

```sh
source-to-markdown ./src ./output.md --language-map ./my-language-map.json
```

## ログ設定

ログ出力にはSerilogを使用しています。
標準ではファイルへ`Information`以上のログを出力します。

設定ファイルは`appsettings.json`です。

例：

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

### 設定項目

| 項目 | 説明 |
| --- | --- |
| `logging.logDirectoryPath` | ログファイル出力先です。`null`の場合は実行ファイル配置先に`log`ディレクトリーを作成します。 |
| `logging.minimumLevel` | ログの最小レベルです。標準は`Information`です。 |
| `logging.retainedFileCountLimit` | 保持するログファイル数です。標準は`31`です。 |

ログファイル名は日次ローリング形式です。

```text
source-to-markdown-YYYYMMDD.log
```


## 除外ルール

以下の項目は出力対象から除外されます。

- `.gitignore`に一致するファイルまたはディレクトリー
- 既定除外ディレクトリー配下
- 隠しファイル
- 隠しディレクトリー配下
- シンボリックリンク
- 出力先Markdownファイル自身
- 未知の拡張子（JSONファイルに定義のないもの）
- 文字コード判定不能なファイル
- バイナリーと判定されたファイル
- 50 MBを超えるファイル


## 終了コード

| 終了コード | 意味 |
| --- | --- |
| `0` | 成功 |
| `1` | 引数エラー |
| `2` | 実行時エラー |
| `3` | 一部スキップまたは警告あり |


## テスト

自動テストは`Tests/source-to-markdown.Tests`にあります。

```sh
dotnet test ./Tests/source-to-markdown.Tests/source-to-markdown.Tests.csproj
```


## ディレクトリー構成・方針

- 単一アプリケーションプロジェクト構成
- テストは`Tests`配下の別プロジェクトとして管理
- Visual Studio固有機能には依存しない
- 責務ごとにフォルダーと名前空間を分割
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


## 注意事項

- 重要な秘密情報がソースディレクトリー内にある場合は出力Markdownを共有する前に必ず内容を確認してください。
- 本ツールはソースコード解析や機密情報マスキングを行いません。


## ライセンス

ライセンスはGPL-3.0（GNU GENERAL PUBLIC LICENSE Version 3）です。
