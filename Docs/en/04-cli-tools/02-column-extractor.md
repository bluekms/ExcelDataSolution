# 4.2 ExcelColumnExtractor

`ExcelColumnExtractor` is a CLI tool that takes Excel files and C# Record definitions as input, picks out only the columns each Record requires, and exports them as CSV. It runs once during the build step, and the Sdp runtime only reads those CSVs.

Where the extractor appears from a record author's point of view is covered in [3.3 Defining Your First Record](../03-usage/03-first-record.md#running-the-extraction). This chapter focuses on the tool itself — the command forms, the full set of options, the output format, and bat examples.

</br></br></br>

## Command Structure

`ExcelColumnExtractor` is a single command. It has no sub-commands (verbs).

```bash
ExcelColumnExtractor.exe [options...]
```

Three required options specify the input folder, the Excel folder, and the output folder.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

</br></br></br>

## Options

|Option|Meaning|Default|
|-|-|-|
|`-r`, `--record-path`|Path to a Record `.cs` file or directory|Required|
|`-e`, `--excel-path`|Path to the directory containing the Excel files|Required|
|`-o`, `--output-path`|Path to the CSV output directory|Required|
|`-s`, `--start-cell`|Header start cell address (e.g., `A1`, `B3`, `C7`)|`A1`|
|`-v`, `--version`|Output version — if specified, produces output in the `output-path/version` subfolder|None|
|`-f`, `--force`|When using `--version`, overwrites even if files already exist in that folder|`false`|
|`-c`, `--encoding`|Output CSV encoding (UTF-8 has no BOM; UTF-16, UTF-32, ASCII, etc.)|`UTF-8`|
|`-l`, `--log-path`|Log directory path (a daily `log<date>.txt` is created under it)|None|
|`-m`, `--min-log-level`|Minimum log level (Verbose, Debug, Information, Warning, Error, Fatal)|Information|

</br></br></br>

## Output Format

The extracted CSV files are created with the **`{file}.{sheet}.csv`** convention.

| Excel File | Sheet | Output CSV |
|-|-|-|
| `GameItems.xlsx` | `Items` | `GameItems.Items.csv` |
| `Heroes.xlsx` | `BaseStats` | `Heroes.BaseStats.csv` |

The CSV header preserves the sheet's **original header** as-is. Even if the Record side maps to a different parameter name with `[ColumnName("Cost")]`, the CSV contains the sheet's `Cost`. The mapping is handled during the load step.

Columns not required by a Record are not included in the CSV. This is exactly why the same Excel can be consumed by the server, the client, and tools, each with a different Record definition.

</br></br></br>

## Header Start Cell (`--start-cell`)

`--start-cell` tells the extractor **where the first header cell is** in each sheet. The row following that cell is treated as data.

|       | **A**             | **B**    | **C**     | **D**   | **E**        |
|-------|-------------------|----------|-----------|---------|--------------|
| **1** | Item Table        |          |           |         |              |
| **2** | Last modified 2026-05-15 |   |           |         |              |
| **3** | Id                | Name     | Memo      | Price   | Category     |
| **4** | 1                 | Potion   | Healing item | 100  | Consumable   |

The sheet above is extracted with `--start-cell A3`. Rows `1` and `2` are a free area (sheet title, change history, etc.) and are ignored.

If the option is omitted, the start is assumed to be `A1`. Within a single project, it is simpler to agree on one start cell.

</br></br>

### Overriding the Start Cell per Record

If most sheets start at the same position but only some sheets need to start elsewhere, write the start cell in the third argument of `[StaticDataRecord]`. When this value is present, it takes precedence over the `--start-cell` CLI option (see [5.2 `[StaticDataRecord]`](../05-advanced/02-attributes.md#attr-staticdatarecord)).

```csharp
// The project default is agreed as B3, but this sheet alone starts at A1
[StaticDataRecord("GameItems", "Quests", "A1")]
public sealed record QuestRecord(int Id, string Title);
```

Keeping the CLI call uniform on a single line and marking only the exceptions with an attribute is easier to manage as the number of sheets grows.

</br></br></br>

## Version Folders (`--version`, `--force`)

If you specify `--version`, the output is gathered in the `output-path/<version>/` subfolder. Use it when you want to separate artifacts by build number or data patch number.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3
```

Result:

```
Csv/
└── 1.2.3/
    ├── GameItems.Items.csv
    ├── Heroes.BaseStats.csv
    └── ...
```

</br></br>

### Things to Watch When Choosing a Version String

If files already exist in the same version folder, extraction **stops with an error** (to prevent unintended overwrites). Therefore, a version string must be **an identifier that, once produced, never yields the same value again**.

An identifier using only the date (`2026-05-18`) is not suitable because it collides every time in a flow that extracts multiple times on the same day. The recommended identifiers are as follows.

- **SemVer + build metadata** — `1.2.3-build.42`, `1.2.3+commit.a1b2c3d`
- **CI build number** — `$(Build.BuildNumber)`, `${{ github.run_number }}`, and other values that CI increments on every build
- **Date + build counter** — `2026-05-18.42` (the Nth build of the same day)
- **Commit hash** — `a1b2c3d` (when keeping artifacts per PR/merge)

If you intentionally need to re-extract into the same version folder (for example, regenerating the same build for debugging purposes), add `--force`.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3 ^
  --force
```

If `--version` is not specified, the conflict check does not run, so `--force` has no meaning either — output goes directly to `output-path`, and files with the same name are simply overwritten. It is common to run without `--version` in local development and separate with `--version` for CI/release artifacts.

</br></br></br>

## Encoding (`--encoding`)

The default is **UTF-8** without a BOM. In most cases you can leave it as-is. If some consumers require UTF-16 or another encoding, specify it.

Supported encodings:

|Value|Meaning|
|-|-|
|`UTF-8`|UTF-8 without a BOM (default)|
|`UTF-16`|UTF-16 LE|
|`UTF-32`|UTF-32|
|`ASCII`|ASCII|
|Others|Handled via .NET `Encoding.GetEncoding(name)`. e.g., `EUC-KR`, `Windows-1252`|

</br></br></br>

## Examples

### Basic

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

### A Project Where the Agreed Start Cell Is `B3`

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3
```

### Separating Artifacts by Build Version

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3 ^
  --version 1.2.3-build.42
```

### Verbose Logging to a File

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --log-path ./Logs ^
  --min-log-level Debug
```

</br></br></br>

## Bundling into a bat File

Since the extractor is called frequently during the build step, it is convenient to bundle it into a bat file.

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

When extraction fails, the exit code is non-zero, so you can branch on `errorlevel`.

If you register the same command as a step in CI such as GitHub Actions, it integrates directly into the build process. When extraction fails, the exit code is non-zero, so it leads straight to a workflow failure, surfacing bad data before it gets merged.

</br></br></br>

## Recommended Workflow

1. Agree on a single `--start-cell` position within a project (e.g., `B3` — column `A` and rows `1`, `2` are the sheet's free area).
2. Place the extractor call as a single step in the build pipeline.
3. The resulting CSVs are copied into the runtime build output folder and read by `StaticDataManager.LoadAsync` (see [3.5](../03-usage/05-static-data-manager.md)).
4. If you want to record the build version in the data, separate the output folder with `--version`.

The validations filtered by the extraction itself are the following four.

- **Record schema defects** — the extractor parses `.cs` files with Roslyn and catches incorrect Attribute usage, unsupported types, and so on (it runs at extractor execution time, not at IDE build time).
- **Missing headers** — when a column required by a Record is not present in the sheet.
- **Cell value-type compatibility** — when a cell value conflicts with the Record's type/Attribute, such as `[Range]`, `[RegularExpression]`, `[DateTimeFormat]`, `[Length]`, `[CountRange]`, enum members, and so on.
- **Primary Key duplication** — duplicate values within the sheet for a column marked with `[Key]`.

Foreign key (`[ForeignKey]`, `[SwitchForeignKey]`) validation happens at runtime (`LoadAsync`), not at the extraction step (see [3.6](../03-usage/06-foreign-keys.md)).

---

[← Previous: 4.1 StaticDataHeaderGenerator](./01-header-generator.md) | [Table of Contents](../README.md) | [Next: 5.1 Supported Types (Schemata) →](../05-advanced/01-schemata.md)
