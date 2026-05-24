# 2. インストール

## 要求環境

- **.NET SDK** が必要 — 正確なバージョンはリポジトリの `global.json` に明記されています

</br></br></br>

## インストール方法

### 1. GitHub Releases から取得する

[Releases ページ](https://github.com/bluekms/StaticDataPipeline/releases) から単一実行ファイルをダウンロードし、好きなパスに配置するか PATH に追加します。

|ファイル|用途|
|-|-|
|`ExcelColumnExtractor-v<バージョン>-win-x64.exe` / `-linux-x64`|Excel → CSV 抽出 CLI|
|`StaticDataHeaderGenerator-v<バージョン>-win-x64.exe` / `-linux-x64`|標準ヘッダー生成 CLI|
|`Sdp.dll`|ランタイムライブラリ — プロジェクトから参照|

CLI の 2 つのツールは `--self-contained` の単一実行ファイルなので、.NET ランタイムのインストールは別途必要ありません。`Sdp.dll` は現在、プロジェクトから直接参照する必要があります（NuGet パッケージ配布は今後サポート予定）。

### 2. ソースビルド

```bash
git clone https://github.com/bluekms/StaticDataPipeline.git
cd StaticDataPipeline
dotnet build -c Release
```

---

[← 前へ: 1. 紹介](./01-introduction.md) | [目次](./README.md) | [次へ: 3.1 Excel を扱う →](./03-usage/01-record-to-excel.md)
