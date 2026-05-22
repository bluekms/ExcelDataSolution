# 3.2 Standard Header Generator

> This chapter is a guide for **data authors**. It covers how to fill in headers automatically, rather than matching them by hand, when a header grows long into a single row — as in the array-of-objects example seen in [3.1](./01-record-to-excel.md).

## Why It Is Needed

Let us recall the two kinds of sheets covered in [3.1](./01-record-to-excel.md).

A simple sheet like `ItemRecord` has a short header that is not hard to write by hand.

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

By contrast, the sheet where a single student has grades for several subjects had a standard header that grew long, as follows.

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |

The Record corresponding to this sheet is as follows.

```csharp
[StaticDataRecord("StudentReport", "Grades")]
public sealed record StudentRecord(
    int Id,
    string Name,
    [Length(3)] ImmutableArray<SubjectScore> Subjects);

public sealed record SubjectScore(string Subject, int Score);
```

If you change the fields of `SubjectScore` or adjust the repetition count, you have to match the header row from scratch again. If there are several sheets, that work multiplies. **`StaticDataHeaderGenerator`** is a CLI tool that takes Record `.cs` files as input and automatically outputs a single header row like the one above.

</br></br></br>

## Trying It Out

We use the `StudentRecord` defined earlier as-is. If the Record `.cs` is in the `./Records` folder, you can obtain a Markdown file containing the standard header with the following single line.

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

A `./Headers/Student.md` file is created, and it contains the following Markdown document.

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

The line you actually paste into Excel is the **single line inside the `### Headers (TSV)` code block**. The full set of options and output formats is laid out in [4.1](../04-cli-tools/01-header-generator.md).

There is also an `all-header` command that extracts every `[StaticDataRecord]` Record in a folder at once. Per-sheet sections are arranged in order within a single Markdown file. The full command form and options are laid out in [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md).

</br></br></br>

## Pasting into Excel

Let us apply the line corresponding to your sheet from the Markdown file above to the Excel header. Assume that, before pasting, the sheet already contains temporary headers and data that the data author wrote in an easy-to-recognize way, as in [3.1](./01-record-to-excel.md).

|       | **A** | **B**   | **C**       | **D**       | **E**       | **F**       | **G**       | **H**       |
|-------|-------|---------|-------------|-------------|-------------|-------------|-------------|-------------|
| **1** | Id    | Name    | MathSubject | MathScore   | EnglishSubject | EnglishScore | ScienceSubject | ScienceScore |
| **2** | 1     | Alice   | Math        | 90          | English     | 85          | Science     | 88          |
| **3** | 2     | Bob     | Math        | 70          | English     | 95          | Science     | 75          |

1. Open `Student.md` in any convenient way — VS Code, Notepad, browser rendering, etc.
2. Select and copy exactly the **single line inside the code block under `### Headers (TSV)`** in that Record's section (do not include the ` ``` ` marker lines). On that line, `Home → Shift+End → Ctrl+C` is usually safe.
3. Open the `StudentReport.xlsx` Excel file and go to the `Grades` sheet.
4. Click once on the cell where the header starts (e.g., `A1`). Only this single cell must be selected.
5. Paste with `Ctrl + V`.

Since the tab delimiters naturally go one per cell into different cells, a single paste replaces the temporary header in row 1 with the standard header.

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |
| **3** | 2     | Bob     | Math                  | 70                  | English               | 95                  | Science               | 75                  |

How much the temporary header (`MathSubject`, `MathScore` …) differs from the standard header (`Subjects[0].Subject` …) is clear when you compare row 1 of the two tables. The data rows are left untouched, and only the single header row has changed.

</br></br></br>

## Recommended Workflow

You do not need to stop data entry even before the standard header is finalized. If you place a **temporary header** one row above, you can carry out Record definition and data entry in parallel.

Suppose you have agreed on the extractor option `--start-cell B3`. In that case, lay out the sheet as follows.

|       | **A**       | **B**       | **C**       | **D**       |
|-------|-------------|-------------|-------------|-------------|
| **1** | (free)       | (free)       | (free)       | (free)       |
| **2** |             | temp header    | temp header    | temp header    |
| **3** |             | standard header    | standard header    | standard header    |
| **4** |             | data       | data       | data       |

- Column `A` and rows `1` and `2` form a free area that the extractor does not read. They are a good place to write information meaningful only within the sheet, such as **notes about this table, change history, or owner memos**.
- In row `B2`, write temporary names that are easy for data authors to recognize (e.g., "ID", "Name", "Math Score").
- Row `B3` is the standard header slot. Leave it empty until the Record definition is finished, then paste in the single line from inside the `### Headers (TSV)` code block of the `StaticDataHeaderGenerator` result Markdown.
- Fill in data starting from `B4`.

The workflow from the data author's perspective proceeds as follows.

1. Agree on the column layout and the starting cell (`B3`) with the record author. At this point the Record `.cs` may still be incomplete.
2. Write the temporary header in `B2`, and start entering data from `B4`.
3. Once the record author finalizes the Record, run `StaticDataHeaderGenerator` to obtain the standard header.
4. Paste that result into `B3`. The temporary header may be left as-is, or cleanly removed.
5. Subsequent extraction proceeds with `ExcelColumnExtractor --start-cell B3` (running it automatically in CI is recommended).

</br></br></br>

## Full Options and Automation

The two command forms (`header` / `all-header`), the full set of options, and a bat automation example are laid out in [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md).

---

[← Previous: 3.1 Working with Excel](./01-record-to-excel.md) | [Table of Contents](../README.md) | [Next: 3.3 Defining Your First Record →](./03-first-record.md)
