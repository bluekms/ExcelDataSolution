# 3.3 Defining Your First Record

> From here on, the perspective shifts to that of the **record author**. Authoring Excel from the data author's perspective was covered in [3.1](./01-record-to-excel.md) and [3.2](./02-header-generator.md); from this chapter on, we look at how to write Records, Tables, and Managers on the C# side.

This is the scenario where a sheet has already been filled in and you are writing the matching C# Record for the first time. Let us assume the example sheet is as follows.

|       | **A**  | **B**    | **C**       | **D**   | **E**        |
|-------|--------|----------|-------------|---------|--------------|
| **1** | Id     | Name     | Memo        | Cost    | Category     |
| **2** | 1      | Potion   | Healing item | 100     | Consumable   |
| **3** | 2      | Sword    | Basic sword  | 5000    | Weapon       |
| **4** | 3      | Shield   | Basic shield | 4000    | Armor        |

`Memo` is a reference column for the data author. It is not used on the C# side. **A column that the Record does not require is not extracted to CSV** — note in advance that `Memo` is missing from the result CSV below.

Suppose the data author calls the price `Cost`, but you want to use the name `Price` in the C# code. In this case you can map the sheet header and the parameter name separately with `[ColumnName]`.

## Record Definition

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

It is short, but it contains all the necessary information. Let us look at it one piece at a time.

### `[StaticDataRecord("GameItems", "Items")]`

This specifies which Excel file and which sheet this Record corresponds to. The first argument is the **Excel file name** (without extension), and the second argument is the **sheet name**. It is used for two purposes.

- When `ExcelColumnExtractor` extracts the CSV, it finds the target file and sheet.
- It is used in the name of the extraction result CSV — `{file}.{sheet}.csv`. In the example above, `GameItems.Items.csv`.

### `int Id`, `string Name`

When there is no special Attribute, the column name is **identical to the parameter name**. The sheet's header must have `Id` and `Name` columns for mapping to occur.

### `[ColumnName("Cost")][Range(0, 1_000_000)] int Price`

`[ColumnName(name)]` tells the mapping when the Excel header name and the C# parameter name differ. The header of the sheet above is `Cost` and the Record parameter is `Price`, so `[ColumnName("Cost")]` connects the two. If the header and the parameter name are the same, there is no need to write it.

`[Range(min, max)]` checks whether a value is within the specified range. It is an Attribute that inherits from `System.ComponentModel.DataAnnotations.RangeAttribute`. Values outside the range are filtered out both at the extraction stage and at runtime load.

> `1_000_000` is C#'s [digit separator](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types#integer-literals) notation, the same value as `1000000`. Since it is only a readability aid, you may also write `[Range(0, 1000000)]`.

### `ItemCategory Category`

An `enum` is **matched by string**. The CSV cell must contain `Consumable` for it to be parsed as `ItemCategory.Consumable`. It is not an integer value, and the casing must match exactly (`consumable`, `CONSUMABLE` fail). An undefined name likewise results in a load failure.

</br></br></br>

## Running the Extraction

Once the Record definition is finished, use **`ExcelColumnExtractor`** to extract CSV from the sheet. The extractor only needs three locations specified — the Record folder, the Excel folder, and the output folder — and it matches and processes all the Records/Excel files within them automatically.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

- `--record-path` — the folder containing the Record `.cs` files with `[StaticDataRecord]` attached
- `--excel-path` — the folder containing the Excel files
- `--output-path` — the folder where the result CSV will be created

If the starting cell location is not `A1`, tell it with `--start-cell`. The full set of options, such as separating outputs per build version (`--version`), changing the encoding (`--encoding`), and log settings, is laid out in [4.2 ExcelColumnExtractor](../04-cli-tools/02-column-extractor.md).

</br></br>

### Result CSV

Only the columns required by the Record above are selected, and `GameItems.Items.csv` is created.

```
Id,Name,Cost,Category
1,Potion,100,Consumable
2,Sword,5000,Weapon
3,Shield,4000,Armor
```

The CSV file name follows the **`{file}.{sheet}.csv`** rule. The `Items` sheet of `GameItems.xlsx` → `GameItems.Items.csv`.

The CSV header keeps the sheet's original header (`Cost`) as-is. At the load stage, `[ColumnName("Cost")]` connects the `Cost` column to the Record's `Price` parameter.

The `Memo` that was in the original sheet is not included in the CSV, because the Record does not require it. This is precisely why the server, the client, and tools can each consume the same Excel with different Record definitions.

</br></br>

### What Is Validated at the Extraction Stage

The extractor does not simply copy cells over; it also checks the following.

- **Flaws in the Record-side schema itself** — the extractor parses the `.cs` files with Roslyn and catches incorrect Attribute usage and the like (it runs at extractor execution time, not as an IDE analyzer).
- **Whether the columns the Record requires exist in the sheet** — if missing, it fails and reports which column of which sheet.
- **Whether the cell values are compatible with the types** — characters in a numeric column, a collection that exceeds the fixed length, `[Range]` / `[RegularExpression]` / format violations, and the like are checked at extraction time.
- **Primary Key duplication** — if the values of a column with `[Key]` attached are duplicated within the sheet, it fails. `[Key]` is not mandatory, and data tables without a PK are also allowed (in that case the duplication check itself is skipped).

Foreign key (`[ForeignKey]`, `[SwitchForeignKey]`) integrity is validated not at the extraction stage but at runtime `LoadAsync` ([3.6](./06-foreign-keys.md)).

</br></br></br>

## Recommended Workflow

1. Agree with the data author on the column layout of the Record `.cs` and the Excel sheet, and the starting cell location.
2. Place the `ExcelColumnExtractor` call as a step in the build pipeline (or a local bat). Whenever the Record changes, you only need to run this step.
3. The produced CSV is copied to the runtime build output, where `StaticDataManager.LoadAsync` reads it ([3.5](./05-static-data-manager.md)).

If you place the extractor call as a CI step, errors that are filtered out at the extraction stage — such as a missing header or a type mismatch — surface automatically before merge, without a person having to run it manually each time.

</br></br></br>

## Next Steps

- To actually load it into memory and query it, create a **StaticDataTable**. This is covered in [3.4](./04-static-data-table.md).
- The full list of usable types and the Attributes that are mandatory for each type are laid out in [5.1 Supported Types](../05-advanced/01-schemata.md).
- The Attribute catalog is collected in [5.2](../05-advanced/02-attributes.md).

---

[← Previous: 3.2 Standard Header Generator](./02-header-generator.md) | [Table of Contents](../README.md) | [Next: 3.4 Implementing StaticDataTable →](./04-static-data-table.md)
