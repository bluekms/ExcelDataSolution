# 3.2 標準ヘッダー生成ツール

> この章は **データ作業者** 向けの案内です。[3.1](./01-record-to-excel.md) で見たオブジェクトの配列の例のように、ヘッダーが 1 行で長くなるとき、手で合わせるのではなく自動で埋める方法を扱います。

## なぜ必要なのか

[3.1](./01-record-to-excel.md) で扱った 2 種類のシートをもう一度思い出してみましょう。

`ItemRecord` のような単純なシートはヘッダーが短く、手で書いても難しくありません。

```csharp
[StaticDataRecord("GameItems", "Items")]
public sealed record ItemRecord(
    int Id,
    string Name,
    int Price,
    ItemCategory Category);
```

|       | **A**  | **B**    | **C**   | **D**        |
|-------|--------|----------|---------|--------------|
| **1** | Id     | Name     | Price   | Category     |
| **2** | 1      | Potion   | 100     | Consumable   |

一方、生徒 1 人が複数科目の成績を持つシートは、標準ヘッダーが次のように長くなりました。

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |

このシートに対応する Record は次のとおりです。

```csharp
[StaticDataRecord("StudentReport", "Grades")]
public sealed record StudentRecord(
    int Id,
    string Name,
    [Length(3)] ImmutableArray<SubjectScore> Subjects);

public sealed record SubjectScore(string Subject, int Score);
```

`SubjectScore` のフィールドを変えたり繰り返し回数を調整したりすると、ヘッダー行を最初から合わせ直さなければなりません。シートが複数あれば、その作業は何倍にも増えます。**`StaticDataHeaderGenerator`** は、Record `.cs` ファイルを入力として受け取り、上記のようなヘッダー 1 行を自動的に出力してくれる CLI ツールです。

</br></br></br>

## 実行してみる

先に定義した `StudentRecord` をそのまま使います。Record `.cs` が `./Records` フォルダにあれば、次の 1 行で標準ヘッダーが入った Markdown ファイルを受け取れます。

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

`./Headers/Student.md` ファイルが生成され、その中には次のような Markdown 文書が入っています。

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

Excel に実際に貼り付ける行は **`### Headers (TSV)` コードブロックの中の 1 行** です。オプションや出力形式の全体は [4.1](../04-cli-tools/01-header-generator.md) にまとめられています。

フォルダ内のすべての `[StaticDataRecord]` Record を一度に抽出する `all-header` コマンドもあります。1 つの Markdown ファイルの中にシートごとのセクションが順に整理されます。コマンド全体の形とオプションは [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md) にまとめられています。

</br></br></br>

## Excel に貼り付ける

上記の Markdown ファイルから自分のシートに該当する行を Excel ヘッダーに適用してみます。貼り付け前のシートには、[3.1](./01-record-to-excel.md) のようにデータ作業者が分かりやすく書いておいた仮ヘッダーとデータがすでに入っているとします。

|       | **A** | **B**   | **C**       | **D**       | **E**       | **F**       | **G**       | **H**       |
|-------|-------|---------|-------------|-------------|-------------|-------------|-------------|-------------|
| **1** | Id    | Name    | 数学科目     | 数学点数     | 英語科目     | 英語点数     | 理科科目     | 理科点数     |
| **2** | 1     | Alice   | Math        | 90          | English     | 85          | Science     | 88          |
| **3** | 2     | Bob     | Math        | 70          | English     | 95          | Science     | 75          |

1. `Student.md` を VS Code、メモ帳、ブラウザレンダリングなど、便利な方法で開きます。
2. 該当する Record セクションの **`### Headers (TSV)` の下のコードブロックの中の 1 行** だけを正確に選択してコピーします (` ``` ` の表示行は含めません)。通常その行で `Home → Shift+End → Ctrl+C` が安全です。
3. Excel の `StudentReport.xlsx` ファイルを開き、`Grades` シートに移動します。
4. ヘッダーが始まるセル (例: `A1`) を一度クリックします。この 1 つのセルだけが選択された状態でなければなりません。
5. `Ctrl + V` で貼り付けます。

タブ区切り文字が自然に 1 マスずつ別のセルに入るため、一度の貼り付けで 1 行目の仮ヘッダーが標準ヘッダーに置き換わります。

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |
| **3** | 2     | Bob     | Math                  | 70                  | English               | 95                  | Science               | 75                  |

仮ヘッダー (`数学科目`、`数学点数` …) と標準ヘッダー (`Subjects[0].Subject` …) がどれだけ異なるかは、2 つの表の 1 行目を比較すると明らかです。データ行はそのままで、ヘッダー 1 行だけが変わりました。

</br></br></br>

## 推奨される作業フロー

標準ヘッダーが確定する前でも、データ入力を止める必要はありません。**仮ヘッダー** を 1 行上に置けば、Record 定義とデータ入力を並行して進められます。

抽出ツールのオプションを `--start-cell B3` で合意したとします。このときシート構成は次のように組みます。

|       | **A**       | **B**       | **C**       | **D**       |
|-------|-------------|-------------|-------------|-------------|
| **1** | (自由)       | (自由)       | (自由)       | (自由)       |
| **2** |             | 仮ヘッダー    | 仮ヘッダー    | 仮ヘッダー    |
| **3** |             | 標準ヘッダー  | 標準ヘッダー  | 標準ヘッダー  |
| **4** |             | データ       | データ       | データ       |

- `A` 列と `1`、`2` 行は抽出ツールが読まない自由領域です。**このテーブルに関する説明書き、変更履歴、担当者メモ** のような、シートの中でのみ意味を持つ情報を書いておくのに良い場所です。
- `B2` 行にはデータ作業者が分かりやすい仮の名前を書いておきます (例: 「ID」「名前」「数学の点数」)。
- `B3` 行は標準ヘッダーの場所です。Record 定義が終わるまでは空けておき、`StaticDataHeaderGenerator` の結果 Markdown の `### Headers (TSV)` コードブロックの中の 1 行をそのまま貼り付けます。
- `B4` からデータを埋めていきます。

データ作業者の立場から見たフローは次のように進みます。

1. レコード作業者とカラム構成、開始セル (`B3`) を合意します。この時点では Record `.cs` がまだ未完成でも構いません。
2. `B2` に仮ヘッダーを書き、`B4` からデータを入力していきます。
3. レコード作業者が Record を確定したら、`StaticDataHeaderGenerator` を実行して標準ヘッダーを受け取ります。
4. その結果を `B3` に貼り付けます。仮ヘッダーはそのまま残しても、きれいに消しても構いません。
5. 以降の抽出は `ExcelColumnExtractor --start-cell B3` で進めます (CI での自動実行を推奨)。

</br></br></br>

## オプション全体と自動化

コマンドの 2 つの形 (`header` / `all-header`)、オプション全体、bat 自動化の例は [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md) にまとめられています。

---

[← 前: 3.1 Excel で作業する](./01-record-to-excel.md) | [目次](../README.md) | [次: 3.3 最初の Record を定義する →](./03-first-record.md)
