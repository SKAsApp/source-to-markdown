# Source to Markdown for AI Chat – AIチャット向けにソースコードをMarkdownファイルへ集約するやつ

[English ver.](./README.en.md)

`source-to-markdown`は指定ディレクトリー配下のソースコードや設定ファイルを1つのMarkdownファイルへ集約し、Microsoft 365 CopilotなどのチャットWeb UIベースの生成AIへ渡しやすくする.NET製のコンソールアプリです。

複数ファイルを個別にアップロードする手間を減らし、ディレクトリー構成を含めたプロジェクト全体のコードを添付できます。また、本ツールが出力したMarkdownから、リポジトリーのディレクトリーとファイルを復元できます。


## 特徴

- 指定ディレクトリー配下を再帰的に探索
- 対象ファイルをMarkdownのコードブロックとして1ファイルへ集約
- 拡張子と言語ヒントの対応をJSONファイルで管理
- 言語ヒントが未登録のテキストファイルも、言語指定なしのコードブロックとして出力
- 拡張子なしのファイルにも対応
- `.gitignore`による除外判定に対応
- 次の既定除外ディレクトリーに対応
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
- UTF-8、UTF-8 with BOM、Shift-JIS、UTF-16 LEの読み込みに対応
- 改行コードをLine Feed（LF）へ正規化
- コードフェンス衝突を回避（ソースコード内に```があるとき、~~~でのコードブロック開始に変更）
- `--reverse`によるMarkdownからのディレクトリー復元
- 復元時のパストラバーサル、重複パス、シンボリックリンク配下への出力を防止
- ファイルへのログ出力
- macOS、Linux、Windowsの同一コード対応


## 動作環境

- OS：macOS、Linux、Windows
- .NET SDK 10 （ビルドする場合）


## インストール

### 方法1：ビルド済みバイナリー（推奨）

[Release](https://github.com/SKAsApp/source-to-markdown/releases)から利用環境のOSのファイルをダウンロードし、任意の場所に展開します。

### 方法2：ソースからビルド

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
dotnet.exe publish ".\source-to-markdown.csproj" --configuration "Release" --runtime "win-x64"
```


## 使い方

### 通常モード

指定ディレクトリーからMarkdownファイルを作成します。

macOS、Linuxの場合：

```sh
source-to-markdown <source-directory> <output-markdown> [options]
```

Windowsの場合：

```powershell
# コンソール出力の文字化け対策
[Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding("utf-8")

# 実行
source-to-markdown.exe <source-directory> <output-markdown> [options]
```

実行例：

```sh
source-to-markdown ./src ./output.md --force
```

カレントディレクトリー全体を`project.md`へ集約する例：

```sh
source-to-markdown ./ ./project.md --force
```

### 逆モード

本ツールが出力したMarkdownから、ディレクトリーとファイルを復元します。

macOS、Linuxの場合：

```sh
source-to-markdown <input-markdown> <output-directory> --reverse [options]
```

Windowsの場合：

```powershell
source-to-markdown.exe <input-markdown> <output-directory> --reverse [options]
```

実行例：

```sh
source-to-markdown ./project.md ./restored-project --reverse
```

復元先に既存ファイルがある場合、`--force`を指定しない限り、ファイルを書き込む前に処理全体を中止します。既存ファイルを一括上書きする場合は、次のように実行します。

```sh
source-to-markdown ./project.md ./restored-project --reverse --force
```


## オプション

| オプション | 通常モード | 逆モード | 説明 |
| --- | --- | --- | --- |
| `--reverse` | モード切り替え | 必須 | Markdownからディレクトリーを復元します。 |
| `--language-map <path>` | 使用可能 | 使用不可 | 拡張子と言語ヒントの対応JSONファイルを指定します。逆モードで指定すると引数エラーになります。 |
| `--verbose` | 使用可能 | 使用可能 | 処理対象、スキップ理由、復元ファイルなどの詳細を表示します。 |
| `--force` | 使用可能 | 使用可能 | 通常モードでは出力Markdownを、逆モードでは復元先の既存ファイルを確認なしで上書きします。 |
| `--help` | 使用可能 | 使用可能 | ヘルプを表示します。 |

未定義のオプション、位置引数の不足または過剰、`--language-map`の値不足は引数エラーになります。


## 出力Markdownの形式

出力Markdownは次の形式になります。

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

---

`settings/custom-setting`

```
独自形式の設定内容
```
~~~

ファイル間の区切りには`---`を出力します。

言語ヒント設定に登録された拡張子には、`csharp`や`json`などの言語ヒントを付けます。設定に登録されていない拡張子や拡張子なしのファイルでも、対応文字コードのテキストであれば、言語ヒントなしのコードブロックとして出力します。

本文に3連バッククォートが含まれる場合はチルダのフェンスへ切り替えます。本文にも同じ長さのチルダがある場合は、衝突しない長さまでフェンスを延長します。

## 逆モードの復元仕様

- 本ツールの形式として認識できるファイルブロックだけを復元します。
- 見出し、説明文、水平区切り、単独のコードブロックなど、認識できないMarkdown部分は無視します。
- Markdownに記載された相対パスとファイル名を使用します。
- 言語ヒントなしのコードブロックでも、拡張子を追加または変更しません。
- 復元したファイルの文字コードはUTF-8、Byte Order Mark（BOM）なしです。
- 復元したファイルの改行コードはLine Feed（LF）です。
- 元ファイルの文字コード、BOM、改行形式はMarkdownに保持されないため、元のバイト列とは一致しない場合があります。
- 絶対パス、復元先外を指すパス、重複パス、ファイルとディレクトリーが衝突するパスを拒否します。
- 復元先外へつながるシンボリックリンク配下には出力しません。
- 不正なパスが1件でもある場合は、復元ファイルを書き込む前に処理全体を中止します。
- 既存ファイルが1件でもあり、`--force`が指定されていない場合は、復元ファイルを書き込む前に処理全体を中止します。

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

独自の言語ヒントJSONを使う場合は`--language-map`を指定します。

```sh
source-to-markdown ./src ./output.md --language-map ./my-language-map.json
```

言語ヒント設定はMarkdownコードブロックの表示にだけ使用します。設定にない拡張子を除外するための一覧ではありません。


## 対応文字コードとテキスト判定

次の文字コードを読み込めます。

- UTF-8
- UTF-8 with BOM
- Shift-JIS
- UTF-16 Little Endian（LE）

空ファイルはテキストファイルとして扱います。文字コードを判定できないファイル、バイナリーファイルの可能性があるファイル、50 MBを超えるファイルはスキップし、警告へ記録します。


## 除外ルール

次の項目は出力対象から除外されます。

- `.gitignore`に一致するファイルまたはディレクトリー
- 既定除外ディレクトリー配下
- 隠しファイル
- 隠しディレクトリー配下
- シンボリックリンク
- 出力先Markdownファイル自身
- 文字コード判定不能なファイル
- バイナリーと判定されたファイル
- 50 MBを超えるファイル

既定除外ファイルについては、処理完了時に`既定除外ファイル数`として件数だけを表示します。`--verbose`指定時は、既定除外されたファイルと理由を確認できます。


## ログ設定

ログ出力にはSerilogを使用します。標準ではファイルへ`Information`以上のログを出力します。設定ファイルは`appsettings.json`です。

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

## 終了コード

| 終了コード | 意味 |
| ---: | --- |
| `0` | 正常終了。既定除外だけが発生した場合も含みます。 |
| `1` | 引数または事前検証エラー |
| `2` | 実行時エラー |
| `3` | 読み取り不能や文字コード判定不能など、既定除外以外の警告あり |


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

- 重要な秘密情報が入力ディレクトリー内にある場合は、出力Markdownを共有する前に必ず内容を確認してください。
- 本ツールはソースコード解析や機密情報マスキングを行いません。
- 逆モードは、信頼できるMarkdownに対して使用してください。安全性検証を行いますが、復元前に出力先と入力内容を確認してください。
- Markdown形式では元の文字コード、BOM、改行形式、ファイル属性、実行権限を完全には保持できません。


## ライセンス

ライセンスはGNU General Public License Version 3（GPL-3.0）です。
