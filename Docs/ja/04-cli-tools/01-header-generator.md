# 4.1 StaticDataHeaderGenerator

`StaticDataHeaderGenerator` は C# Record 定義から **標準ヘッダー** を抽出する CLI ツールです。オブジェクト配列のようにヘッダーが長くなるシートのヘッダー行を手作業で揃えることなく、自動で埋められるようにします。

結果は **Markdown ドキュメント** として生成されます。1 つのファイル内に Record 単位でセクションが作られ、その中にヘッダー一覧 (List) と区切り文字でつないだヘッダー行 (Code block) が一緒に含まれます。データ作業者は Code block 内の 1 行をコピーして Excel ヘッダーに貼り付けます ([3.2 標準ヘッダージェネレーター](../03-usage/02-header-generator.md) 参照)。

この章はツール自体の使い方 — コマンドの形式、オプション全体、出力形式、bat の例 — に焦点を当てます。

</br></br></br>

## コマンド構造

コマンドには 2 つの形式があり、最初の引数でどの形式かを指定します。

```bash
StaticDataHeaderGenerator.exe header [オプション...]
StaticDataHeaderGenerator.exe all-header [オプション...]
```

- `header` — **Record 1 つ** の標準ヘッダーを生成します。`--record-name` で対象を指定する必要があります。
- `all-header` — `--record-path` フォルダー以下のすべての `[StaticDataRecord]` Record の標準ヘッダーを一括で生成します。

`header` は結果の Markdown をコンソールに出力し、`--output-file` を指定するとそのファイルにも保存します (コンソール出力はそのまま維持)。`all-header` はフォルダー全体を処理して出力量が多くなる可能性があるため、コンソールには出力せず、`--output-file` で指定したファイルにのみ保存します。

</br></br></br>

## オプション

### `header` — 単一 Record

|オプション|意味|デフォルト値|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` ファイルまたはディレクトリのパス|必須|
|`-n`, `--record-name`|対象 Record の名前 (クラス名、例: `StudentRecord`)|必須|
|`-s`, `--separator`|Code block 内のヘッダー間に入れる区切り文字|`\t` (タブ)|
|`-o`, `--output-file`|出力ファイルのパス (なければコンソール)|なし|
|`-l`, `--log-path`|ログディレクトリのパス (その下に日付別の `log<日付>.txt` が生成される)|なし|
|`-m`, `--min-log-level`|最小ログレベル (Verbose, Debug, Information, Warning, Error, Fatal)|Information|

### `all-header` — フォルダー全体

|オプション|意味|デフォルト値|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` ファイルまたはディレクトリのパス|必須|
|`-s`, `--separator`|Code block 内のヘッダー間に入れる区切り文字|`\t` (タブ)|
|`-o`, `--output-file`|出力ファイルのパス (`all-header` はコンソール出力がないため、省略すると結果がファイルに残らない)|なし|
|`-l`, `--log-path`|ログディレクトリのパス (その下に日付別の `log<日付>.txt` が生成される)|なし|
|`-m`, `--min-log-level`|最小ログレベル|Information|

`all-header` には `--record-name` がありません。フォルダー全体を処理するため、対象の指定は不要です。

`--output-file` に拡張子を付けずパスだけを与えると、自動的に `.md` が付きます。拡張子を指定しても、出力内容は常に Markdown です。

</br></br></br>

## 出力形式

結果は次の構造の Markdown ドキュメントです。

```markdown
# StaticDataHeaderGenerator Results

## {RecordFullName}
- Excel File: `{ExcelFileName}.xlsx`
- Sheet Name: `{SheetName}`

### Headers (List)
- Id
- Name
- ...

### Headers (TSV)
​```
Id<sep>Name<sep>...
​```
```

- 最上部に `# StaticDataHeaderGenerator Results` の 1 行。
- Record ごとに `## {RecordFullName}` セクションが 1 つずつ。`{RecordFullName}` は Record が名前空間内に宣言されている場合、`名前空間.型名` の形になります (このドキュメントの例の Record は名前空間なしで定義されていると仮定し、単純な名前で表記)。
  - `Excel File`, `Sheet Name` — `[StaticDataRecord]` の 2 つの引数。
  - `### Headers (List)` — ヘッダーを 1 行に 1 つずつ bullet で。
  - `### Headers (TSV)` — `--separator` でつないだ 1 行をコードブロック内に表記。
  
`--separator` が影響を与える箇所は **`### Headers` セクションのラベルとその下のコードブロック内の 1 行** だけです。ラベルはタブなら `(TSV)`、カンマなら `(CSV)`、それ以外は括弧なしの `### Headers` に決まります。ヘッダー一覧 (List) や他のメタ情報はそのまま維持されます。

</br></br></br>

## 実行例

このセクションでは、`./Records` フォルダー内に次の 2 つの Record があると仮定します。

```csharp
using System.Collections.Immutable;
using Sdp.Attributes;

[StaticDataRecord("StudentReport", "Grades")]
public sealed record StudentRecord(
    int Id,
    string Name,
    [Length(3)] ImmutableArray<SubjectScore> Subjects);

public sealed record SubjectScore(string Subject, int Score);

[StaticDataRecord("GameItems", "Items")]
public sealed record ItemRecord(
    int Id,
    string Name,
    int Price,
    ItemCategory Category);
```

</br></br>

### 単一 Record のヘッダー — ファイル出力

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

結果の `./Headers/Student.md`:

````markdown
# StaticDataHeaderGenerator Results

## StudentRecord
- Excel File: `StudentReport.xlsx`
- Sheet Name: `Grades`

### Headers (List)
- Id
- Name
- Subjects[0].Subject
- Subjects[0].Score
- Subjects[1].Subject
- Subjects[1].Score
- Subjects[2].Subject
- Subjects[2].Score

### Headers (TSV)
```
Id	Name	Subjects[0].Subject	Subjects[0].Score	Subjects[1].Subject	Subjects[1].Score	Subjects[2].Subject	Subjects[2].Score
```
````

`### Headers (TSV)` コードブロック内の 1 行をコピーして Excel ヘッダーの最初のセルに貼り付けると、自動的に展開されます ([3.2 — Excel に貼り付ける](../03-usage/02-header-generator.md#excel-に貼り付ける))。

</br></br>

### 単一 Record のヘッダー — コンソール出力

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord
```

`--output-file` を省略すると、上記の Markdown ドキュメントがコンソールにのみ出力されます。

</br></br>

### フォルダー全体 — 1 ファイルに

```bash
StaticDataHeaderGenerator.exe all-header ^
  --record-path ./Records ^
  --output-file ./Headers/AllHeaders.md
```

`./Records` 以下のすべての `[StaticDataRecord]` Record が 1 つの Markdown ファイル内にシート別セクションとして整理されます。結果の `./Headers/AllHeaders.md`:

````markdown
# StaticDataHeaderGenerator Results

## ItemRecord
- Excel File: `GameItems.xlsx`
- Sheet Name: `Items`

### Headers (List)
- Id
- Name
- Price
- Category

### Headers (TSV)
```
Id	Name	Price	Category
```

## StudentRecord
- Excel File: `StudentReport.xlsx`
- Sheet Name: `Grades`

### Headers (List)
- Id
- Name
- Subjects[0].Subject
- Subjects[0].Score
- Subjects[1].Subject
- Subjects[1].Score
- Subjects[2].Subject
- Subjects[2].Score

### Headers (TSV)
```
Id	Name	Subjects[0].Subject	Subjects[0].Score	Subjects[1].Subject	Subjects[1].Score	Subjects[2].Subject	Subjects[2].Score
```
````

データ作業者は自分のシートに該当するセクションを探し、`### Headers (TSV)` 内の 1 行をコピーして Excel に貼り付けます。

</br></br>

### 区切り文字の変更

デフォルトの区切り文字はタブですが、カンマや他の文字に変えられます。

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --separator , ^
  --output-file ./Headers/Student.md
```

同じ `StudentRecord` を 2 つの区切り文字で抽出した結果を比較すると、**`### Headers` セクションのラベルとその下のコードブロック内の 1 行** だけが変わります。

タブ区切り (`--separator` 省略、デフォルト):

````markdown
### Headers (TSV)
```
Id	Name	Subjects[0].Subject	Subjects[0].Score	...
```
````

カンマ区切り (`--separator ,`):

````markdown
### Headers (CSV)
```
Id,Name,Subjects[0].Subject,Subjects[0].Score,...
```
````

ドキュメントの残りの部分 (タイトル、`Excel File`、`Sheet Name`、`Headers (List)`) はそのままです。タブやカンマ以外の区切り文字を使うと、ラベルは `### Headers` (括弧なし) と表記されます。

Excel に貼り付けるときは **タブ区切りが最も便利です** — 1 つのセルに貼り付けると自動的に隣のセルに展開されます。カンマなど他の区切り文字は、Excel の「区切り位置」のような変換ステップがもう 1 回必要になることがあります。

</br></br>

#### ヘッダーに区切り文字が含まれていると生成が阻止されます

選択した区切り文字がいずれかのヘッダー名の中にそのまま含まれていると (例: `--separator ,` なのに `Sub,Total` のようなヘッダーが生成される場合)、貼り付けた後にカラムが誤って分割され、データの整合性が崩れます。ヘッダージェネレーターはこのような衝突を検出すると `InvalidOperationException` で即座に中断し、衝突したヘッダーの一覧をメッセージに含めて報告します。通常は record パラメーター名や `[ColumnName]` の値に区切り文字が含まれないように整理すれば解決します。

</br></br></br>

## bat にまとめておく

毎回オプションを覚えて入力しなくて済むように、**1 つの bat ファイル** を Record フォルダーの隣に置いておくと便利です。

```bat
@echo off
StaticDataHeaderGenerator.exe all-header ^
  --record-path .\Records ^
  --output-file .\Headers\AllHeaders.md
pause
```

`pause` があると結果メッセージを確認した後にウィンドウが閉じるため、ダブルクリックで安心して実行できます。結果ファイルはすべてのシートのヘッダーセクションを集めた Markdown ドキュメントです。

ローカルでは bat が便利ですが、同じコマンドを GitHub Actions のような CI の 1 ステップとして登録しておけば、ビルド工程にそのまま統合できます。

</br></br></br>

## 推奨ワークフロー

1. Record `.cs` がある程度確定したら、`all-header` で全ヘッダーを 1 つの Markdown ファイルに抽出します。
2. データ作業者がそのファイルから自分のシートに該当するセクションを探し、`### Headers (TSV)` 内の 1 行をコピーして Excel に貼り付けます ([3.2](../03-usage/02-header-generator.md#excel-に貼り付ける))。
3. Record が変わるたびに bat で再抽出すればよいです — 同じ場所に新しい Markdown ファイルが作られ、データ作業者は同じ場所で更新されたヘッダーを受け取ります。

CI 環境でヘッダージェネレーターを実行して結果の Markdown ファイルを成果物としてアップロードしておけば、データ作業者は常に最新版を同じ場所で受け取れます。

---

[← 前: 3.7 StaticDataView 事前生成ビュー](../03-usage/07-static-data-view.md) | [目次](../README.md) | [次: 4.2 ExcelColumnExtractor →](./02-column-extractor.md)
