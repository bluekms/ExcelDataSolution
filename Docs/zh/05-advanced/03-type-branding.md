# 5.3 类型品牌化模式

类型品牌化（type branding）是一种让编译器区分含义不同的 ID 的技法。如果将同为 `int` 的 `CharId` 和 `ItemId` 原样保留，编译器会把它们视为相同类型，无法捕获错误的赋值。把它们包装成各自独立的类型，错误的赋值就会在构建时被阻止。

在 Sdp 中表达类型品牌化有两种方法。**这两种方式是替代关系**，并不是一起使用的模式，而是根据情况二选一的方式。

---

## 方法 1 —— 单一原始参数的 record struct

这是用一层 record struct 包装原始类型的形态。

```csharp
public record struct CharId(int Value);
public record struct ItemId(int Value);
```

即便包装的是相同的 `int`，由于 record struct 类型不同，错误的赋值会被作为编译错误捕获。

```csharp
void GetItem(ItemId id) { ... }

var charId = new CharId(100);
GetItem(charId);   // 编译错误 —— 不能把 CharId 放到期望 ItemId 的位置
```

### CSV 表头的视角

当参数仅有一个且其类型为原始类型时，如 `record struct CharId(int Value)`，表头会合并为一格父列。从数据作业者的视角看它就是一个普通的整数列，只有 C# 一侧将其作为强类型包装对象接收。

```csharp
[StaticDataRecord("Game", "Heroes")]
public sealed record HeroRecord(
    [Key] CharId Id,
    string Name);
```

生成的表头：

```
Id    Name
```

只要在 CSV 单元格中写下 `100`，就会映射为 `new CharId(100)`。此外，由于附加了 [`[Key]`](./02-attributes.md#attr-key)，`ExcelColumnExtractor` 还会在提取阶段一并检查该列的值是否重复。

---

</br></br></br>

## 方法 2 —— enum

enum 本身就是一个独立的类型，因此不会与其他 enum 或整数混淆。

```csharp
public enum SkillId
{
    Fireball = 1001,
    Heal = 1002,
    Lightning = 1003,
}
```

如果在 enum 成员上明确指定整数值，该值就会成为写入 CSV 单元格的值。如果 CSV 单元格中写着 `1001`，而 enum 中定义了 `Fireball = 1001`，映射结果就是 `SkillId.Fireball`。数据作业者在工作表中写下普通整数 `1001`，C# 代码则可以用 `SkillId.Fireball` 这个名称来处理同一行 —— 而无需把魔法数字 `1001` 散落在代码各处。

反之，对于 enum 中没有名称的值，例如单元格中写着 `1004` 但没有与该值对应的成员，则会映射为 `(SkillId)1004`。你可以这样使用：只挑选经常引用的 ID 作为 enum 成员命名，其余的则保持整数值不变（不过，要允许无名称的值，必须在下文所述的 `[Key]` 位置使用）。

### `[Key]` enum 的成员检查省略

作为 `[Key]` 使用的 enum 在映射时会省略 `Enum.IsDefined` 检查。也就是说，未定义为 enum 成员的整数值（`1004`、`9999` 等）也会被接受并映射为 `(SkillId)1004`。

得益于这一行为，可以将 enum 用作 **ID 编码空间** 而非 **封闭集合**。数据作业者每次添加新的技能 ID 时，无需更新 enum 成员，只需为少数已知的值命名，其余的作为数据添加即可。

```csharp
[StaticDataRecord("Game", "Skills")]
public sealed record SkillRecord(
    [Key] SkillId Id,   // 像 (SkillId)9999 这样的未定义值也会原样映射
    string Name);
```

> 不是 `[Key]` 的普通 enum 参数会应用 `Enum.IsDefined` 并拒绝未定义的值。要将 enum 用于类型品牌化，必须在 `[Key]` 位置使用。

---

</br></br></br>

## 两种方式的对比

|视角|单参数 record|enum|
|-|-|-|
|声明形态|`record struct CharId(int Value);`|`enum SkillId { ... }`|
|在代码中取值|`id.Value`|`(int)id`|
|为已知值命名|不可能（值仅来自数据）|可自然地表达为 enum 成员（`SkillId.Fireball`）|
|自由添加新 ID|没有问题（值为任意整数）|仅当为 `[Key]` 时没有问题（检查被省略）|
|适合的情形|纯粹的 ID，在代码中频繁创建、传递|部分 ID 拥有命名常量而其余作为数据添加|

---

</br></br></br>

## 与外键的结合

品牌化类型可以直接用作 `[ForeignKey]` 的目标。

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

由于 `AssignedTo` 是 `CharId` 类型，不会与其他 ID 混淆，同时还会在加载时点验证 `HeroTable.Id` 列中是否存在实际的值。

---

[← 上一篇: 5.2 Attribute 目录](./02-attributes.md) | [目录](../README.md) | [下一篇: 5.4 校验概述 →](./04-validation.md)
