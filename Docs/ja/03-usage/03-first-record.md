# 3.3 最初の Record を定義する

> ここからは **レコード作業者** の視点に切り替わります。データ作業者の視点での Excel 作成は [3.1](./01-record-to-excel.md)、[3.2](./02-header-generator.md) で扱い、この章からは C# 側で Record と Table、Manager をどう書くかを見ていきます。

すでに埋められたシートがあり、それに合った C# Record を初めて書くシナリオです。例のシートは次のようだとします。

|       | **A**  | **B**    | **C**       | **D**   | **E**        |
|-------|--------|----------|-------------|---------|--------------|
| **1** | Id     | Name     | Memo        | Cost    | Category     |
| **2** | 1      | Potion   | 回復アイテム  | 100     | Consumable   |
| **3** | 2      | Sword    | 基本の剣     | 5000    | Weapon       |
| **4** | 3      | Shield   | 基本の盾     | 4000    | Armor        |

`Memo` はデータ作業者の参考用カラムです。C# 側では使用しません。**Record が要求しないカラムは CSV に抽出されません** — 下記の結果 CSV で `Memo` が抜けるという点をあらかじめ見ておいてください。

データ作業者は価格を `Cost` と呼んでいますが、C# コードでは `Price` という名前を使いたいとします。この場合 `[ColumnName]` でシートヘッダーとパラメータ名を分離してマッピングできます。

## Record 定義

```csharp
using Sdp.Attributes;

public enum ItemCategory
{
    Consumable,
    Weapon,
    Armor,
}

[StaticDataRecord("GameItems", "Items")]
public sealed record ItemRecord(
    int Id,
    string Name,
    [ColumnName("Cost")][Range(0, 1_000_000)] int Price,
    ItemCategory Category);
```

短いですが、必要な情報がすべて入っています。1 つずつ見ていきましょう。

### `[StaticDataRecord("GameItems", "Items")]`

この Record がどの Excel ファイルのどのシートに対応するかを指定します。第 1 引数が **Excel ファイル名** (拡張子を除く)、第 2 引数が **シート名** です。2 つの用途で使われます。

- `ExcelColumnExtractor` が CSV を抽出するとき、対象ファイルとシートを見つける。
- 抽出結果 CSV のファイル名 — `{ファイル}.{シート}.csv` — に使われる。上記の例では `GameItems.Items.csv`。

### `int Id`, `string Name`

特別な Attribute がなければ、カラム名は **パラメータ名と同一** です。シートのヘッダーに `Id`、`Name` カラムがないとマッピングされません。

### `[ColumnName("Cost")][Range(0, 1_000_000)] int Price`

`[ColumnName(name)]` は Excel ヘッダー名と C# パラメータ名が異なるとき、そのマッピングを伝えます。上記シートのヘッダーは `Cost` で Record パラメータは `Price` なので、`[ColumnName("Cost")]` で 2 つを結びつけます。ヘッダーとパラメータ名が同じであれば、わざわざ書かなくても構いません。

`[Range(min, max)]` は値が指定した範囲の中にあるかを検査します。`System.ComponentModel.DataAnnotations.RangeAttribute` を継承した Attribute です。範囲を外れた値は抽出ステージとランタイムロードの両方でふるい落とされます。

> `1_000_000` は C# の [数値リテラル区切り文字](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types#integer-literals) の表記で、`1000000` と同じ値です。可読性の補助でしかないので `[Range(0, 1000000)]` と書いても構いません。

### `ItemCategory Category`

`enum` は **文字列でマッチング** されます。CSV セルに `Consumable` と書かれていないと `ItemCategory.Consumable` としてパースされません。整数値ではなく、大文字小文字も正確に一致しなければなりません (`consumable`、`CONSUMABLE` は失敗)。定義されていない名前も同様にロード失敗です。

</br></br></br>

## 抽出を実行する

Record 定義が終わったら **`ExcelColumnExtractor`** でシートから CSV を抽出します。抽出ツールは Record フォルダ、Excel フォルダ、出力フォルダの 3 つだけを指定すれば、その中のすべての Record/Excel を自動でマッチングして処理します。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

- `--record-path` — `[StaticDataRecord]` が付いた Record `.cs` ファイルがあるフォルダ
- `--excel-path` — Excel ファイルがあるフォルダ
- `--output-path` — 結果 CSV が作成されるフォルダ

開始セルの位置が `A1` でなければ `--start-cell` で伝えます。ビルドバージョンごとの成果物の分離 (`--version`)、エンコーディングの変更 (`--encoding`)、ログ設定のようなオプション全体は [4.2 ExcelColumnExtractor](../04-cli-tools/02-column-extractor.md) にまとめられています。

</br></br>

### 結果 CSV

上記の Record が要求するカラムだけを選り分けて `GameItems.Items.csv` が作成されます。

```
Id,Name,Cost,Category
1,Potion,100,Consumable
2,Sword,5000,Weapon
3,Shield,4000,Armor
```

CSV ファイル名は **`{ファイル}.{シート}.csv`** のルールです。`GameItems.xlsx` の `Items` シート → `GameItems.Items.csv`。

CSV のヘッダーはシートの元のヘッダー (`Cost`) をそのまま維持します。ロードステージで `[ColumnName("Cost")]` が `Cost` カラムを Record の `Price` パラメータに結びつけてくれます。

元のシートにあった `Memo` は Record が要求しないので CSV には含まれません。同じ Excel をサーバー、クライアント、ツールがそれぞれ異なる Record 定義で消費できる理由がここにあります。

</br></br>

### 抽出ステージで検証されるもの

抽出ツールは単にセルを書き写すだけでなく、次のものも一緒に検査します。

- **Record 側のスキーマ自体の欠陥** — 抽出ツールが Roslyn で `.cs` ファイルをパースし、誤った Attribute の使用などを検出します (IDE のアナライザーではなく抽出ツールの実行時点で動作)。
- **Record が要求するカラムがシートにあるか** — なければ失敗し、どのシートのどのカラムかを報告します。
- **セル値が型と互換性があるか** — 数値カラムに文字が入っていたり、定められた長さを外れたコレクション、`[Range]` / `[RegularExpression]` / フォーマット違反などを抽出時点で検査します。
- **Primary Key の重複** — `[Key]` が付いたカラムの値がシート内で重複すると失敗します。`[Key]` は必須ではなく、PK のないデータテーブルも許可されます (この場合、重複検査自体が省略されます)。

外部キー (`[ForeignKey]`、`[SwitchForeignKey]`) の整合性は抽出ステージではなく、ランタイムの `LoadAsync` で検証されます ([3.6](./06-foreign-keys.md))。

</br></br></br>

## 推奨される作業フロー

1. Record `.cs` と Excel シートのカラム構成、開始セルの位置をデータ作業者と合意します。
2. ビルドパイプライン (またはローカルの bat) に `ExcelColumnExtractor` 呼び出しを 1 ステップとして置きます。Record が変わるたびにこのステップだけ実行すればよいです。
3. 生成された CSV はランタイムビルド出力にコピーされ、`StaticDataManager.LoadAsync` が読み込みます ([3.5](./05-static-data-manager.md))。

抽出ツールの呼び出しを CI ステージに置けば、ヘッダーの欠落や型の不一致のように抽出ステージでふるい落とされるエラーが、人が毎回手動で実行しなくてもマージ前に自動的にあらわになります。

</br></br></br>

## 次のステップ

- 実際にメモリにロードして照会するには **StaticDataTable** を作ります。[3.4](./04-static-data-table.md) で扱います。
- 使用可能な型の全リストと、各型に必須で付いてくる Attribute は [5.1 サポート型](../05-advanced/01-schemata.md) でまとめます。
- Attribute カタログは [5.2](../05-advanced/02-attributes.md) に集められています。

---

[← 前: 3.2 標準ヘッダー生成ツール](./02-header-generator.md) | [目次](../README.md) | [次: 3.4 StaticDataTable の実装 →](./04-static-data-table.md)
