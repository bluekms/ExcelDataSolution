# 5.4 バリデーション概要

Sdp のバリデーションがパイプラインのどの時点で動作するのかを 1 ページで整理します。個々の Attribute の詳細は [5.2 Attribute カタログ](./02-attributes.md) を、データフロー全体は [1. 紹介](../01-introduction.md) を参照してください。

## 3 つの検査時点

|時点|実行主体|対象|
|-|-|-|
|**宣言検査**|`ExcelColumnExtractor` · `StaticDataHeaderGenerator`|Record/Attribute の **宣言** 自体の欠陥|
|**抽出**|`ExcelColumnExtractor`|Excel の **セル値** がスキーマ・Attribute と互換かどうか|
|**ロード**|`StaticDataManager.LoadAsync`|ランタイム読み込み時点での構造・値・参照の検査|

宣言検査と抽出はビルドパイプライン（オフライン）で、ロードはアプリケーションランタイムで動作します。宣言検査は `.cs` ソースを解析するため `ExcelColumnExtractor` と `StaticDataHeaderGenerator` の両方の実行時に行われ、ソースのないランタイムでは動作しません。

</br></br></br>

## 検査項目ごとの動作時点

`O` はその時点で検査することを、`—` は検査しないことを意味します。

### A. 型・構造スキーマ

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|サポート対象型かどうか|O|—|—|未サポート型を拒否|
|コレクション自体の nullable 禁止|O|—|—|`ImmutableArray<T>?`·`FrozenSet<T>?`·`FrozenDictionary<,>?`|
|Nullable な Record 要素の禁止|O|—|—|Record 配列/セット/Map Value の nullable|
|循環参照の禁止|O|—|—|Record が自分自身を直接・間接に含む|
|Map Key non-nullable|O|—|—||
|Map Key↔Value `[Key]` 型の一致|O|—|—||

### B. Attribute 整合性

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|`[Length]` 必須|O|—|—|複数カラムコレクション|
|`[NullString]` 必須・誤用|O|—|—|nullable に必須、non-nullable に禁止|
|`[DateTimeFormat]` 必須・誤用|O|—|—|`DateTime` に必須、非 DateTime に禁止|
|`[TimeSpanFormat]` 必須・誤用|O|—|—|`TimeSpan` に必須、非 TimeSpan に禁止|
|`[RegularExpression]` の型|O|—|—|`string` / `string?` 専用|
|`[Range]` 適用可能な型|O|—|—|`bool`·コレクション·record は禁止|
|`[CountRange]` 整合性|O|—|—|`[SingleColumnCollection]` 必須、`[Length]` と排他、minCount>0|
|`[SingleColumnCollection]` 整合性|O|—|—|Map 不可、要素は primitive のみ|
|`[Key]` 整合性|O|—|O|Record あたり最大 1 個・non-nullable は宣言検査 / Map Value の `[Key]` の存在は宣言検査・ロードの両方|
|`[StaticDataRecord]` の存在|O|O|O|宣言検査対象の識別 / 抽出対象が 0 個のとき終了 / ロード時に必須|

### C. セル値

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|ヘッダーの存在|—|O|O||
|セル値と型の互換性|—|O|O|ロードは変換（`Convert.ChangeType`）の失敗で検出|
|enum メンバー名の有効性|—|O|O|ロードは `Enum.IsDefined` — `[Key]` enum は省略|
|`DateTime`/`TimeSpan` フォーマットの一致|—|O|O|両側で同一 format による `ParseExact`|
|`[Range]` 値の範囲|—|O|O||
|`[RegularExpression]` パターンの一致|—|O|O||
|`[CountRange]` 分割要素の個数|—|O|O|単一カラムコレクション|
|Primary Key のシート内重複|—|O|—|`[Key]` カラム。ロードは自動検査なし — `UniqueIndex` が opt-in で保証|

### D. 外部キー

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|`[ForeignKey]`·`[SwitchForeignKey]` の同時付与禁止|O|—|O|両側で検査|
|`[SwitchForeignKey]` の重複条件禁止|O|—|O|両側で検査|
|FK ターゲット TableSet の存在|—|—|O|名前のタイプミスはロード前の型検査で、`disabledTables` で外されたテーブルへの参照はロード後の参照検証で検出（どちらも `FkTargetNotFound`）|
|FK ターゲットが `[SingleColumnCollection]` でないこと|—|—|O||
|FK ターゲットカラムの存在|—|—|O||
|`[SwitchForeignKey]` 条件カラムの存在|—|—|O||
|`[SwitchForeignKey]` 条件値の分岐マッチング|—|—|O||
|FK 参照値の存在|—|—|O|実際の参照整合性|

### E. ロード構造

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|TableSet の単一コンストラクター|—|—|O||
|テーブルパラメーターの型|—|—|O|`StaticDataTable<,>` かどうか|
|テーブルコンストラクター（`ImmutableArray`）の存在|—|—|O||
|ViewSet の単一コンストラクター|—|—|O||
|View パラメーターの型・non-nullable|—|—|O||
|View コンストラクター（`TableSet`）の存在|—|—|O||
|`LoadAsync` への同時進入禁止|—|—|O||
|`UniqueIndex` キーの重複|—|—|O|テーブル/ビュー生成時、opt-in|

### F. ユーザー定義検証

|検査項目|宣言検査|抽出|ロード|備考|
|-|-|-|-|-|
|テーブルの自己検証|—|—|O|`StaticDataTable.Validate()` override、テーブルインスタンス化直後|
|マネージャーのクロス検証|—|—|O|`StaticDataManager.Validate(TTableSet)` override、すべての FK 検証後|
|ビューの自己検証|—|—|O|`StaticDataView.Validate()` override、ビュービルド直後|

</br></br></br>

## 抽出を経た CSV のみを運用に載せる

セル値検査（C）のうち `[Range]`·`[RegularExpression]`·`[CountRange]` は抽出とロードの両方で同じルールで動作します。しかし Primary Key の重複や宣言整合性の検査（A·B）は抽出段階（宣言検査を含む）でのみ行われます。抽出段階を経ていない CSV をランタイムに直接投入すると、こうした検査が抜け落ちるため、正常なパイプラインでは常に抽出器を経た CSV のみを運用に載せます。

---

[← 前へ: 5.3 タイプブランディングパターン](./03-type-branding.md) | [目次](../README.md) | [次へ: 6. ライセンス →](../06-license.md)
