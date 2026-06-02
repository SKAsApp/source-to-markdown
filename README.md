# source-to-markdown

指定ディレクトリー配下のソースコードや設定ファイルを、1つのMarkdownファイルへ集約する.NET 10コンソールアプリです。

## 実行例

```bash
dotnet run --project source-to-markdown.csproj -- ./src ./output.md --force
```

## オプション

- `--language-map <path>`: 拡張子と言語ヒントの対応JSONを指定します。
- `--verbose`: 処理中ファイル、スキップ理由、採用文字コード、採用言語ヒントを表示します。
- `--force`: 既存Markdownファイルを確認なしで上書きします。
- `--help`: ヘルプを表示します。

## 補足

Markdownのファイル区切りはファイル間のみ `---` を出力し、最後のファイル後には出力しません。
