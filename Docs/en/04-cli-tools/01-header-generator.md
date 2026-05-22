# 4.1 StaticDataHeaderGenerator

`StaticDataHeaderGenerator` is a CLI tool that extracts a **standard header** from a C# Record definition. It lets you fill in the header row of a sheet automatically — instead of aligning headers by hand — even when the header grows long, such as with object arrays.

The result is produced as a **Markdown document**. Within a single file, a section is created per Record, and inside each section you get both a header list (List) and a header line joined by a separator (Code block). The data author copies a single line from the Code block and pastes it into the Excel header (see [3.2 Standard Header Generator](../03-usage/02-header-generator.md)).

This chapter focuses on the tool itself — the command forms, the full set of options, the output format, and bat examples.

</br></br></br>

## Command Structure

There are two command forms, and the first argument specifies which form is used.

```bash
StaticDataHeaderGenerator.exe header [options...]
StaticDataHeaderGenerator.exe all-header [options...]
```

- `header` — generates the standard header for **a single Record**. You must specify the target with `--record-name`.
- `all-header` — generates standard headers for every `[StaticDataRecord]` Record under the `--record-path` folder at once.

`header` prints the resulting Markdown to the console, and if `--output-file` is specified, it also saves to that file (the console output is still kept). `all-header` processes an entire folder and may produce a large amount of output, so it does not print to the console and only saves to the file specified by `--output-file`.

</br></br></br>

## Options

### `header` — Single Record

|Option|Meaning|Default|
|-|-|-|
|`-r`, `--record-path`|Path to a Record `.cs` file or directory|Required|
|`-n`, `--record-name`|Target Record name (the class name, e.g., `StudentRecord`)|Required|
|`-s`, `--separator`|Separator placed between headers inside the Code block|`\t` (tab)|
|`-o`, `--output-file`|Output file path (console if omitted)|None|
|`-l`, `--log-path`|Log directory path (a daily `log<date>.txt` is created under it)|None|
|`-m`, `--min-log-level`|Minimum log level (Verbose, Debug, Information, Warning, Error, Fatal)|Information|

### `all-header` — Entire Folder

|Option|Meaning|Default|
|-|-|-|
|`-r`, `--record-path`|Path to a Record `.cs` file or directory|Required|
|`-s`, `--separator`|Separator placed between headers inside the Code block|`\t` (tab)|
|`-o`, `--output-file`|Output file path (`all-header` has no console output, so omitting it leaves no file result)|None|
|`-l`, `--log-path`|Log directory path (a daily `log<date>.txt` is created under it)|None|
|`-m`, `--min-log-level`|Minimum log level|Information|

`all-header` has no `--record-name`. Since it processes the entire folder, no target needs to be specified.

If you give `--output-file` only a path without an extension, `.md` is appended automatically. Even if you specify an extension, the output content is always Markdown.

</br></br></br>

## Output Format

The result is a Markdown document with the following structure.

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

- A single top line, `# StaticDataHeaderGenerator Results`.
- One `## {RecordFullName}` section per Record. `{RecordFullName}` takes the form `Namespace.TypeName` if the Record is declared inside a namespace (the example Records in this document are assumed to be defined without a namespace, so they are written as simple names).
  - `Excel File`, `Sheet Name` — the two arguments of `[StaticDataRecord]`.
  - `### Headers (List)` — headers as bullets, one per line.
  - `### Headers (TSV)` — a single line joined by `--separator`, shown inside a code block.

The only place `--separator` affects is **the label of the `### Headers` section and the single line inside the code block below it**. The label is determined as `(TSV)` for a tab, `(CSV)` for a comma, and otherwise just `### Headers` with no parentheses. The header list (List) and other metadata stay unchanged.

</br></br></br>

## Examples

In this section, assume the `./Records` folder contains the following two Records.

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

### Header for a Single Record — File Output

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

Resulting `./Headers/Student.md`:

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

Copy the single line inside the `### Headers (TSV)` code block and paste it into the first cell of the Excel header — it expands automatically ([3.2 — Pasting into Excel](../03-usage/02-header-generator.md#pasting-into-excel)).

</br></br>

### Header for a Single Record — Console Output

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord
```

If you omit `--output-file`, the Markdown document above is printed to the console only.

</br></br>

### Entire Folder — Into a Single File

```bash
StaticDataHeaderGenerator.exe all-header ^
  --record-path ./Records ^
  --output-file ./Headers/AllHeaders.md
```

Every `[StaticDataRecord]` Record under `./Records` is organized into per-sheet sections within a single Markdown file. Resulting `./Headers/AllHeaders.md`:

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

The data author finds the section corresponding to their own sheet, copies the single line inside `### Headers (TSV)`, and pastes it into Excel.

</br></br>

### Changing the Separator

The default separator is a tab, but you can change it to a comma or another character.

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --separator , ^
  --output-file ./Headers/Student.md
```

If you compare the results of extracting the same `StudentRecord` with two different separators, only **the label of the `### Headers` section and the single line inside the code block below it** change.

Tab separator (`--separator` omitted, default):

````markdown
### Headers (TSV)
```
Id	Name	Subjects[0].Subject	Subjects[0].Score	...
```
````

Comma separator (`--separator ,`):

````markdown
### Headers (CSV)
```
Id,Name,Subjects[0].Subject,Subjects[0].Score,...
```
````

The rest of the document (the title, `Excel File`, `Sheet Name`, `Headers (List)`) stays unchanged. If you use a separator other than a tab or comma, the label is written as `### Headers` (without parentheses).

When pasting into Excel, **a tab separator is the most convenient** — pasting into a single cell automatically expands it across the adjacent cells. Other separators, such as a comma, may require an additional conversion step like Excel's "Text to Columns".

</br></br>

#### Generation Is Blocked If a Header Contains the Separator

If the chosen separator appears verbatim inside any header name (for example, `--separator ,` while a header like `Sub,Total` is produced), pasting it would split columns incorrectly and break data consistency. When the header generator detects such a conflict, it stops immediately with an `InvalidOperationException` and reports the list of conflicting headers in the message. This is usually resolved by cleaning up record parameter names or `[ColumnName]` values so they do not contain the separator.

</br></br></br>

## Bundling into a bat File

To avoid memorizing and typing the options every time, it is convenient to keep **a single bat file** next to the Record folder.

```bat
@echo off
StaticDataHeaderGenerator.exe all-header ^
  --record-path .\Records ^
  --output-file .\Headers\AllHeaders.md
pause
```

With `pause`, the window stays open after the result message so you can review it, which makes running it by double-click safe. The result file is a Markdown document gathering the header sections of all sheets.

A bat file is convenient locally, but if you register the same command as a step in CI such as GitHub Actions, it integrates directly into the build process.

</br></br></br>

## Recommended Workflow

1. Once the Record `.cs` files are reasonably finalized, use `all-header` to extract all headers into a single Markdown file.
2. The data author finds the section corresponding to their own sheet in that file, copies the single line inside `### Headers (TSV)`, and pastes it into Excel ([3.2](../03-usage/02-header-generator.md#pasting-into-excel)).
3. Whenever a Record changes, just re-extract with the bat file — a new Markdown file is created in the same location, and the data author receives the updated header in the same place.

If you run the header generator in a CI environment and publish the resulting Markdown file as an artifact, the data author can always receive the latest version in the same place.

---

[← Previous: 3.7 StaticDataView Precomputed Views](../03-usage/07-static-data-view.md) | [Table of Contents](../README.md) | [Next: 4.2 ExcelColumnExtractor →](./02-column-extractor.md)
