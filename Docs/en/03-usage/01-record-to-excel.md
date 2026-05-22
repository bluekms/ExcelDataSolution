# 3.1 Working with Excel

> This chapter is a guide for **data authors**. It explains how an Excel sheet should be laid out so that Sdp picks it up as-is. One of Sdp's strengths is that, apart from the standard header, you can organize the sheet however is most convenient for you. The C# side of authoring a Record is covered in [3.3 Defining Your First Record](./03-first-record.md).

## Example Sheet

The sheet we will work with throughout this chapter looks like this.

|       | **A**  | **B**    | **C**       | **D**   | **E**        |
|-------|--------|----------|-------------|---------|--------------|
| **1** | Id     | Name     | Memo        | Price   | Category     |
| **2** | 1      | Potion   | Healing item | 100     | Consumable   |
| **3** | 2      | Sword    | Basic sword  | 5000    | Weapon       |
| **4** | 3      | Shield   | Basic shield | 4000    | Armor        |

- **Row 1 is the header**, and **data starts from row 2**.
- `Memo` is a comment column that only data authors see. A column that has not been communicated to the record author is simply ignored, so you can keep it freely within the sheet (see [Reference Columns Can Be Left As-Is](#reference-columns-can-be-left-as-is) for details).
- For an **enum column** like `Category`, where you choose one value from a predetermined list, you write the value name as-is.

Once the casing of a column name has been agreed upon with the record author, keep it that way.

</br></br></br>

## The Cell Where the Header Starts

The first header of the sheet does not have to be `A1`. **It may start at any cell.** If you want to put a sheet title or a note about what data the sheet contains above or to the left, you can offset the header by that much.

|       | **A** | **B**             | **C**    | **D**     | **E**   | **F**        |
|-------|-------|-------------------|----------|-----------|---------|--------------|
| **1** |       | Item Table        |          |           |         |              |
| **2** |       | Basic info of items shown in the shop | |    |         |              |
| **3** |       | Id                | Name     | Memo      | Price   | Category     |
| **4** |       | 1                 | Potion   | Healing item | 100  | Consumable   |

In this case the starting cell is `B3`. Column `A` and rows `1` and `2` form a free area that the extractor does not read, making them a good place to write a sheet title or a description of what data this table holds. The starting cell location only needs to be agreed upon once with the record author, and it is simpler if every sheet in a project uses the same starting position.

</br></br></br>

## Reference Columns Can Be Left As-Is

Columns for remarks or notes that you want to see only within the sheet are frequently needed. **A column that has not been communicated to the record author is simply ignored during extraction.** The `Memo` column in the example above is such a case — it means you can keep notes shared among data authors right in the sheet.

Conversely, if an agreed-upon column is **missing from the sheet, extraction fails.**

</br></br></br>

## enum Columns

A column like `Category`, where you choose one value from a predetermined list, is called an **enum column**. You write the **value name** directly in the cell.

|       | **A**  | **B**    | **C**        |
|-------|--------|----------|--------------|
| **1** | Id     | Name     | Category     |
| **2** | 1      | Potion   | Consumable   |
| **3** | 2      | Sword    | Weapon       |

The notation must match **exactly, including casing**. `consumable`, `CONSUMABLE`, and names not in the list (such as `Magic`) all result in extraction failure.

The value list is information you receive by agreement with the record author.

</br></br></br>

## Commas, Line Breaks, and Double Quotes in String Cells

You may put commas, line breaks (`Alt+Enter`), and double quotes directly into string cells. They are preserved exactly as they appear in the sheet, with no extra processing.

|       | **A**  | **B**    | **C**                              |
|-------|--------|----------|------------------------------------|
| **1** | Id     | Name     | Description                        |
| **2** | 1      | Potion   | Restores HP and MP                 |
| **3** | 2      | Letter   | Reads "Hello"<br>(with a line break) |

The `Description` cell in row 3 contains a value with a line break inserted via `Alt+Enter` inside the Excel cell. This line break is preserved exactly as it appears.

</br></br></br>

## Multiple Values of the Same Kind — Collections

Sometimes you need to hold multiple values of the same kind in a single row. For example, the list of tags an item has. Choose whichever of the two representations suits the nature of the data. Which one to use only needs to be agreed upon once with the record author.

### Approach A — Bundle in One Cell with a Delimiter

If the tags are merely labels and there is no need to review individual values, **bundling them in one cell with a delimiter** is nicely lightweight.

|       | **A**  | **B**    | **C**               |
|-------|--------|----------|---------------------|
| **1** | Id     | Name     | Tags                |
| **2** | 1      | Potion   | heal, consumable    |
| **3** | 2      | Sword    | melee, iron         |
| **4** | 3      | Shield   | defense, iron       |

</br>

The delimiter is not limited to a single comma character. Not only single characters such as `|`, `;`, and ` / `, but also multi-character strings such as `, ` and ` - ` can be used as delimiters. It just needs to not overlap with the values in the sheet, and which delimiter to use only needs to be agreed upon once with the record author. For example, if a tag contains a comma, you can agree on ` | ` (a single space on each side) as the delimiter, as below, to make it more readable at a glance.

|       | **A**  | **B**    | **C**                       |
|-------|--------|----------|-----------------------------|
| **1** | Id     | Name     | Tags                        |
| **2** | 1      | Potion   | heal \| consumable, small   |
| **3** | 2      | Sword    | melee \| iron, starter      |

</br>

- A row is short and bundles together visibly at a glance.
- **Even if the number of items grows later, it applies as-is without any agreement** — you just add items to the cell.
- However, sorting and filtering by individual item is difficult within Excel itself.

</br></br>

### Approach B — Spread Across Multiple Cells

If you want to review each tag individually or sort and filter at the cell level, **spreading them across multiple cells** is advantageous.

|       | **A**  | **B**    | **C**     | **D**        | **E**     |
|-------|--------|----------|-----------|--------------|-----------|
| **1** | Id     | Name     | Tags[0]   | Tags[1]      | Tags[2]   |
| **2** | 1      | Potion   | heal      | consumable   | small     |
| **3** | 2      | Sword    | melee     | iron         | starter   |
| **4** | 3      | Shield   | defense   | iron         | starter   |

</br>

- Excel's filter and sort features can treat each item independently.
- Since the number of columns is fixed, increasing or decreasing it requires an agreement with the record author.
- When you want to leave a cell empty, you only need to agree once on which token (`"-"`, `empty cell`, etc.) represents an empty value.

</br></br>

The choice between the two approaches can be thought of as follows.

- If the number of columns is likely to grow or shrink frequently, **Approach A** is advantageous for ease of management.
- If the number of columns is fixed and you want to make use of cell-level sorting/filtering, **Approach B** is advantageous.

</br></br></br>

## Multiple Objects — Arrays of Objects

There are cases where, instead of simple values like tags, a **bundle with multiple fields** appears multiple times in a single row. For example, a sheet where a single student has grades for several subjects. While working, it is natural to first write things down with temporary headers that are easy to recognize.

Instead of laying out fields side by side like `MathSubject`, `MathScore`, `EnglishSubject`, `EnglishScore` …, if you bundle such data into a **collection of objects**, the C# code can process it with a single `foreach` loop that iterates as many times as there are subjects. Laying out fields means the code grows and has to be handled item by item every time an item is added, but with a collection, the same loop works as-is regardless of the number of items. If a single row contains multiple bundles of the same kind, writing them as a collection is almost always advantageous.

|       | **A** | **B**   | **C**        | **D**     | **E**          | **F**       | **G**         | **H**      |
|-------|-------|---------|--------------|-----------|----------------|-------------|---------------|------------|
| **1** | Id    | Name    | MathSubject  | MathScore | EnglishSubject | EnglishScore | ScienceSubject | ScienceScore |
| **2** | 1     | Alice   | Math         | 90        | English        | 85          | Science       | 88         |
| **3** | 2     | Bob     | Math         | 70        | English        | 95          | Science       | 75         |
| **4** | 3     | Carol   | Math         | 80        | English        | 80          | Science       | 90         |

However, for automatic extraction to work, the header must be in a fixed standard form. The standard header for the sheet above becomes long, as follows.

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |

Matching this standard header by hand is no small task. Once the data agreement is finished, the **standard header generator** fills this part in automatically. We will continue with that in the next chapter.

---

[← Previous: 2. Installation](../02-installation.md) | [Table of Contents](../README.md) | [Next: 3.2 Standard Header Generator →](./02-header-generator.md)
