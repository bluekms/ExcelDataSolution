# 5.3 Type Branding Pattern

Type branding is a technique that makes the compiler distinguish between IDs that have different meanings. If you leave `CharId` and `ItemId`, both of which are `int`, as-is, the compiler treats them as the same type and cannot catch an incorrect assignment. Wrapping them in separate types blocks incorrect assignments at build time.

There are two ways to express type branding in Sdp. **The two approaches are alternatives**; they are not a pattern to be used together, but rather a choice between one or the other depending on the situation.

---

## Approach 1 — single primitive parameter record struct

This form wraps a primitive type in a single layer of record struct.

```csharp
public record struct CharId(int Value);
public record struct ItemId(int Value);
```

Even though they wrap the same `int`, the record struct types differ, so an incorrect assignment is caught as a compile error.

```csharp
void GetItem(ItemId id) { ... }

var charId = new CharId(100);
GetItem(charId);   // Compile error — a CharId cannot go where an ItemId is expected
```

### CSV header perspective

When a parameter is the only one and its type is primitive, as in `record struct CharId(int Value)`, the header collapses into a single parent column. From the data author's perspective it is an ordinary integer column, and only the C# side receives it as a strongly typed wrapper object.

```csharp
[StaticDataRecord("Game", "Heroes")]
public sealed record HeroRecord(
    [Key] CharId Id,
    string Name);
```

Generated header:

```
Id    Name
```

If you write just `100` in the CSV cell, it maps to `new CharId(100)`. Furthermore, because [`[Key]`](./02-attributes.md#attr-key) is attached, `ExcelColumnExtractor` also checks this column for duplicate values at the extraction stage.

---

</br></br></br>

## Approach 2 — enum

An enum is itself a distinct type, so it does not mix with other enums or integers.

```csharp
public enum SkillId
{
    Fireball = 1001,
    Heal = 1002,
    Lightning = 1003,
}
```

If you specify an integer value on an enum member, that value becomes exactly what is written in the CSV cell. If `1001` is written in a CSV cell and the enum defines `Fireball = 1001`, the mapping result is `SkillId.Fireball`. The data author writes the plain integer `1001` in the sheet, and the C# code can handle the same row under the name `SkillId.Fireball` — without scattering the magic number `1001` throughout the code.

Conversely, for a value that has no name in the enum — for example, if `1004` is written in a cell but there is no member corresponding to that value — it maps to `(SkillId)1004`. You can use this so that only the frequently referenced IDs are picked out and given names as enum members, while the rest are left as plain integer values (however, to allow unnamed values, it must be used in the `[Key]` position described below).

### Skipping the member check for a `[Key]` enum

An enum used as `[Key]` has its `Enum.IsDefined` check omitted during mapping. That is, integer values not defined as enum members (`1004`, `9999`, etc.) are also accepted and map to `(SkillId)1004`.

Thanks to this behavior, you can use an enum not as a **closed set** but as an **ID code space**. Each time the data author adds a new skill ID, there is no need to update the enum members; you can name only the few already-known values and add the rest as data.

```csharp
[StaticDataRecord("Game", "Skills")]
public sealed record SkillRecord(
    [Key] SkillId Id,   // An undefined value like (SkillId)9999 is also mapped as-is
    string Name);
```

> A regular enum parameter that is not `[Key]` has `Enum.IsDefined` applied and rejects undefined values. To use an enum for type branding, it must be used in the `[Key]` position.

---

</br></br></br>

## Comparison of the two approaches

|Aspect|single-parameter record|enum|
|-|-|-|
|Declaration form|`record struct CharId(int Value);`|`enum SkillId { ... }`|
|Extracting the value in code|`id.Value`|`(int)id`|
|Naming known values|Not possible (values come only from data)|Naturally expressed as enum members (`SkillId.Fireball`)|
|Freely adding new IDs|No problem (values are arbitrary integers)|No problem only when `[Key]` (the check is omitted)|
|Suitable situation|Pure IDs, frequently created and passed in code|Some IDs have named constants while the rest are added as data|

---

</br></br></br>

## Combining with foreign keys

A branded type can be used directly as the target of `[ForeignKey]`.

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

Because `AssignedTo` is of type `CharId`, there is no chance of it mixing with other IDs, and at the same time it is validated at load time whether the actual value exists in the `HeroTable.Id` column.

---

[← Previous: 5.2 Attribute Catalog](./02-attributes.md) | [Table of Contents](../README.md) | [Next: 5.4 Validation Overview →](./04-validation.md)
