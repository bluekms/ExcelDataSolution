# 5.3 タイプブランディングパターン

タイプブランディング（type branding）は、意味の異なる ID 同士をコンパイラーが区別できるようにする手法です。どちらも `int` である `CharId` と `ItemId` をそのままにしておくと、コンパイラーは同じ型とみなし、誤った代入を捕まえてくれません。別々の型で包めば、誤った代入がビルド時点でブロックされます。

Sdp でタイプブランディングを表現する方法は 2 つあります。**2 つの方式は代替関係** であり、一緒に使うパターンではなく、状況に応じてどちらか一方を選ぶ方式です。

---

## 方法 1 — 単一のプリミティブパラメーターを持つ record struct

プリミティブ型を record struct で 1 枚包んだ形です。

```csharp
public record struct CharId(int Value);
public record struct ItemId(int Value);
```

同じ `int` を包んでも record struct の型が異なるため、誤った代入がコンパイルエラーとして捕まります。

```csharp
void GetItem(ItemId id) { ... }

var charId = new CharId(100);
GetItem(charId);   // コンパイルエラー — CharId を ItemId の位置に入れられない
```

### CSV ヘッダーの観点

`record struct CharId(int Value)` のようにパラメーターがただ 1 つで、その型がプリミティブの場合、ヘッダーは親カラム 1 マスにまとまります。データ作業者から見れば普通の整数カラムであり、C# 側でのみ強い型の包みオブジェクトとして受け取ります。

```csharp
[StaticDataRecord("Game", "Heroes")]
public sealed record HeroRecord(
    [Key] CharId Id,
    string Name);
```

生成されるヘッダー:

```
Id    Name
```

CSV のセルに `100` とだけ書けば `new CharId(100)` にマッピングされます。また [`[Key]`](./02-attributes.md#attr-key) が付いているため、`ExcelColumnExtractor` が抽出段階でこのカラムの値の重複も併せて検査します。

---

</br></br></br>

## 方法 2 — enum

enum はそれ自体が別個の型なので、他の enum や整数と混ざりません。

```csharp
public enum SkillId
{
    Fireball = 1001,
    Heal = 1002,
    Lightning = 1003,
}
```

enum メンバーに整数値を明示しておくと、その値がそのまま CSV のセルに書かれる値になります。CSV のセルに `1001` と書かれていて、enum に `Fireball = 1001` と定義されていれば、マッピング結果は `SkillId.Fireball` になります。データ作業者はシートに普通の整数 `1001` を書き、C# コードは同じ行を `SkillId.Fireball` という名前で扱えます — マジックナンバー `1001` をコードのあちこちに散らさずに済みます。

逆に enum に名前のない値、たとえばセルに `1004` と書かれているのにその値に対応するメンバーがなければ `(SkillId)1004` にマッピングされます。よく参照する ID だけを選んで enum メンバーとして名前を付け、残りは整数値のまま置いておく、という使い方ができます（ただし、名前のない値を許可するには下記の `[Key]` の位置で使う必要があります）。

### `[Key]` enum のメンバー検査の省略

`[Key]` として使われた enum は、マッピング時に `Enum.IsDefined` 検査が省略されます。つまり enum メンバーとして定義されていない整数値（`1004`、`9999` など）も受け入れられ、`(SkillId)1004` にマッピングされます。

この動作のおかげで、enum を **閉じた集合** ではなく **ID コード空間** として活用できます。データ作業者が新しいスキル ID を追加するたびに enum メンバーを更新する必要はなく、すでに知られている一部の値だけ命名しておき、残りはデータとして追加すればよいのです。

```csharp
[StaticDataRecord("Game", "Skills")]
public sealed record SkillRecord(
    [Key] SkillId Id,   // (SkillId)9999 のような未定義値もそのままマッピング
    string Name);
```

> `[Key]` でない一般の enum パラメーターには `Enum.IsDefined` が適用され、未定義値を拒否します。タイプブランディングとして enum を使うには `[Key]` の位置で使用しなければなりません。

---

</br></br></br>

## 2 つの方式の比較

|観点|単一パラメーター record|enum|
|-|-|-|
|宣言の形|`record struct CharId(int Value);`|`enum SkillId { ... }`|
|コードからの値の取り出し|`id.Value`|`(int)id`|
|既知の値への命名|不可能（値はデータからのみ来る）|enum メンバーとして自然に表現（`SkillId.Fireball`）|
|新しい ID の自由な追加|問題なし（値は任意の整数）|`[Key]` のときのみ問題なし（検査が省略される）|
|適した状況|純粋な ID、コードで頻繁に生成・受け渡し|一部の ID に命名された定数があり、残りはデータとして追加|

---

</br></br></br>

## 外部キーとの結合

ブランディング型は `[ForeignKey]` の対象としてそのまま使用できます。

```csharp
public record struct CharId(int Value);

[StaticDataRecord("Game", "Heroes")]
public sealed record HeroRecord(
    [Key] CharId Id,
    string Name);

[StaticDataRecord("Game", "Quests")]
public sealed record QuestRecord(
    [Key] int Id,
    [ForeignKey("HeroTable", "Id")] CharId AssignedTo);
```

`AssignedTo` が `CharId` 型なので他の ID と混ざることがなく、同時に `HeroTable.Id` カラムに実際の値が存在するかどうかがロード時点で検証されます。

---

[← 前へ: 5.2 Attribute カタログ](./02-attributes.md) | [目次](../README.md) | [次へ: 5.4 バリデーション概要 →](./04-validation.md)
