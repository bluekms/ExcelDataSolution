# 4.2 ExcelColumnExtractor

`ExcelColumnExtractor` は Excel ファイル群と C# Record 定義を入力として受け取り、各 Record が要求するカラムだけを抜き出して CSV に書き出す CLI ツールです。ビルド段階で一度実行し、Sdp ランタイムはその CSV だけを読みます。

Record 作業者の視点で抽出器がどこに登場するかは [3.3 最初の Record を定義する](../03-usage/03-first-record.md#抽出を実行する) で扱います。この章はツール自体の使い方 — コマンドの形式、オプション全体、出力形式、bat の例 — に焦点を当てます。

</br></br></br>

## コマンド構造

`ExcelColumnExtractor` は単一のコマンドです。サブコマンド (verb) はありません。

```bash
ExcelColumnExtractor.exe [オプション...]
```

必須オプション 3 つで入力フォルダー、Excel フォルダー、出力フォルダーを指定します。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

</br></br></br>

## オプション

|オプション|意味|デフォルト値|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` ファイルまたはディレクトリのパス|必須|
|`-e`, `--excel-path`|Excel ファイルがあるディレクトリのパス|必須|
|`-o`, `--output-path`|CSV 出力ディレクトリのパス|必須|
|`-s`, `--start-cell`|ヘッダー開始セルのアドレス (例: `A1`, `B3`, `C7`)|`A1`|
|`-v`, `--version`|出力バージョン — 指定すると `output-path/version` サブフォルダーに出力される|なし|
|`-f`, `--force`|`--version` 使用時、そのフォルダーにファイルが既にあっても上書きする|`false`|
|`-c`, `--encoding`|出力 CSV のエンコーディング (UTF-8 は BOM なし、UTF-16, UTF-32, ASCII など)|`UTF-8`|
|`-l`, `--log-path`|ログディレクトリのパス (その下に日付別の `log<日付>.txt` が生成される)|なし|
|`-m`, `--min-log-level`|最小ログレベル (Verbose, Debug, Information, Warning, Error, Fatal)|Information|

</br></br></br>

## 出力形式

抽出結果の CSV は **`{ファイル}.{シート}.csv`** という規則で作られます。

| Excel ファイル | シート | 出力 CSV |
|-|-|-|
| `GameItems.xlsx` | `Items` | `GameItems.Items.csv` |
| `Heroes.xlsx` | `BaseStats` | `Heroes.BaseStats.csv` |

CSV のヘッダーはシートの **元のヘッダー** をそのまま維持します。Record 側で `[ColumnName("Cost")]` で別のパラメーター名にマッピングしても、CSV にはシートの `Cost` が入ります。マッピングはロード段階で処理されます。

Record が要求しないカラムは CSV に含まれません。同じ Excel をサーバー、クライアント、ツールがそれぞれ異なる Record 定義で消費できる理由がここにあります。

</br></br></br>

## ヘッダー開始セル (`--start-cell`)

`--start-cell` は各シートで **ヘッダーの先頭セルがどこか** を抽出器に伝えます。そのセルの次の行からデータとみなします。

|       | **A**             | **B**    | **C**     | **D**   | **E**        |
|-------|-------------------|----------|-----------|---------|--------------|
| **1** | アイテムテーブル    |          |           |         |              |
| **2** | 最終更新 2026-05-15 |        |           |         |              |
| **3** | Id                | Name     | Memo      | Price   | Category     |
| **4** | 1                 | Potion   | 回復アイテム | 100     | Consumable   |

上のシートは `--start-cell A3` で抽出します。`1`, `2` 行は自由領域 (シートのタイトル、変更履歴など) なので無視されます。

オプションを省略すると `A1` から始まると仮定します。1 つのプロジェクト内では開始セルを 1 つに合意しておく方が単純です。

</br></br>

### Record 単位で開始セルを上書きする

ほとんどのシートは同じ位置から始まるが一部のシートだけ別の位置から始める必要がある場合は、`[StaticDataRecord]` の 3 番目の引数に開始セルを書きます。この値があると `--start-cell` CLI オプションより優先されます ([5.2 `[StaticDataRecord]`](../05-advanced/02-attributes.md#attr-staticdatarecord) 参照)。

```csharp
// プロジェクトのデフォルトは B3 で合意されているが、このシートだけ A1 から始まる
[StaticDataRecord("GameItems", "Quests", "A1")]
public sealed record QuestRecord(int Id, string Title);
```

CLI 呼び出しは 1 行に統一しておき、例外だけを attribute で示す方式が、シート数が増えたときに管理しやすいです。

</br></br></br>

## バージョンフォルダー (`--version`, `--force`)

`--version` を指定すると、出力は `output-path/<version>/` サブフォルダーに集まります。ビルド番号やデータパッチ番号で成果物を分けておきたいときに使います。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3
```

結果:

```
Csv/
└── 1.2.3/
    ├── GameItems.Items.csv
    ├── Heroes.BaseStats.csv
    └── ...
```

</br></br>

### バージョン文字列を決めるときの注意点

同じバージョンフォルダーに既にファイルがあると、抽出は **エラーで中断** されます (意図しない上書きの防止)。したがってバージョン文字列は **一度作られたら同じ値が再び出ない識別子** でなければなりません。

日付だけを使う識別子 (`2026-05-18`) は、同じ日に複数回抽出する流れで毎回衝突するため適しません。推奨される識別子は次のとおりです。

- **SemVer + ビルドメタデータ** — `1.2.3-build.42`, `1.2.3+commit.a1b2c3d`
- **CI ビルド番号** — `$(Build.BuildNumber)`, `${{ github.run_number }}` など CI がビルドごとに増加させる値
- **日付 + ビルドカウンター** — `2026-05-18.42` (同じ日の N 番目のビルド)
- **コミットハッシュ** — `a1b2c3d` (PR/マージ単位で成果物を保管するとき)

同じバージョンフォルダーに意図的に再抽出する必要がある場合 (例: デバッグ目的で同じビルドを再生成) は `--force` を追加します。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3 ^
  --force
```

`--version` を指定しないと衝突チェックが動作しないため、`--force` も意味がありません — そのまま `output-path` に出力し、同じ名前のファイルはそのまま上書きされます。ローカル開発では `--version` なしで実行し、CI/配布の成果物では `--version` で分けるのが一般的です。

</br></br></br>

## エンコーディング (`--encoding`)

デフォルト値は BOM なしの **UTF-8** です。ほとんどの場合そのままで構いません。一部の消費者が UTF-16 や他のエンコーディングを要求するなら指定します。

サポートされるエンコーディング:

|値|意味|
|-|-|
|`UTF-8`|BOM なしの UTF-8 (デフォルト)|
|`UTF-16`|UTF-16 LE|
|`UTF-32`|UTF-32|
|`ASCII`|ASCII|
|その他|.NET `Encoding.GetEncoding(name)` で処理。例: `EUC-KR`, `Windows-1252`|

</br></br></br>

## 実行例

### 基本

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

### 開始セルの合意が `B3` のプロジェクト

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3
```

### ビルドバージョン別に成果物を分ける

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3 ^
  --version 1.2.3-build.42
```

### ログをファイルに残しながら詳細に

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --log-path ./Logs ^
  --min-log-level Debug
```

</br></br></br>

## bat にまとめておく

抽出器はビルド段階で頻繁に呼び出されるため、bat にまとめておくと便利です。

```bat
@echo off
ExcelColumnExtractor.exe ^
  --record-path .\Records ^
  --excel-path .\Excels ^
  --output-path .\Csv ^
  --start-cell B3
if errorlevel 1 (
  echo Extract failed.
  pause
  exit /b 1
)
echo Extract succeeded.
pause
```

抽出失敗時は終了コードが 0 ではないため、`errorlevel` で分岐できます。

同じコマンドを GitHub Actions のような CI の 1 ステップとして登録しておけば、ビルド工程にそのまま統合されます。抽出失敗時は終了コードが 0 ではないため、そのままワークフローの失敗につながり、誤ったデータがマージされる前に表面化します。

</br></br></br>

## 推奨ワークフロー

1. 1 つのプロジェクト内で `--start-cell` の位置を 1 つに合意します (例: `B3` — `A` 列と `1`, `2` 行はシートの自由領域)。
2. ビルドパイプラインに抽出器の呼び出しを 1 ステップとして置きます。
3. 出力 CSV はランタイムのビルド出力フォルダーにコピーされ、`StaticDataManager.LoadAsync` が読みます ([3.5](../03-usage/05-static-data-manager.md) 参照)。
4. ビルドバージョンをデータに記録しておきたい場合は `--version` で出力フォルダーを分けます。

抽出自体でふるい落とす検証は次の 4 つです。

- **Record スキーマの欠陥** — 抽出器が Roslyn で `.cs` ファイルをパースし、誤った Attribute の使用、サポートされていない型などを捕捉します (IDE のビルド時点ではなく抽出器の実行時点で動作)。
- **ヘッダー欠落** — Record が要求したカラムがシートにないとき。
- **セル値と型の互換性** — `[Range]`, `[RegularExpression]`, `[DateTimeFormat]`, `[Length]`, `[CountRange]`, enum メンバーなど、セル値が Record の型/Attribute と食い違うとき。
- **Primary Key の重複** — `[Key]` が付いたカラムのシート内の値の重複。

外部キー (`[ForeignKey]`, `[SwitchForeignKey]`) の検証は抽出段階ではなくランタイム (`LoadAsync`) で起こります ([3.6](../03-usage/06-foreign-keys.md) 参照)。

---

[← 前: 4.1 StaticDataHeaderGenerator](./01-header-generator.md) | [目次](../README.md) | [次: 5.1 サポート型 (Schemata) →](../05-advanced/01-schemata.md)
