# 3.2 标准表头生成器

> 本章是面向 **数据作业者** 的指南。如 [3.1](./01-record-to-excel.md) 中所见的对象数组示例那样，当表头延伸成长长的一行时，本章介绍如何不靠手工对齐而是自动填充的方法。

## 为什么需要它

让我们再次回想 [3.1](./01-record-to-excel.md) 中介绍的两类工作表。

像 `ItemRecord` 这样简单的工作表表头很短，手写也不困难。

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

相比之下，一名学生持有多个科目成绩的工作表，其标准表头会如下变得很长。

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |

与该工作表对应的 Record 如下。

```csharp
[StaticDataRecord("StudentReport", "Grades")]
public sealed record StudentRecord(
    int Id,
    string Name,
    [Length(3)] ImmutableArray<SubjectScore> Subjects);

public sealed record SubjectScore(string Subject, int Score);
```

如果更改 `SubjectScore` 的字段或调整重复次数，就必须从头重新对齐表头行。如果有多个工作表，这项工作会成倍增加。**`StaticDataHeaderGenerator`** 是一个 CLI 工具，它以 Record `.cs` 文件作为输入，自动输出上述那样的一行表头。

</br></br></br>

## 试着运行

我们直接使用前面定义的 `StudentRecord`。如果 Record `.cs` 位于 `./Records` 文件夹中，可以用下面这一行获取包含标准表头的 Markdown 文件。

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

会生成一个 `./Headers/Student.md` 文件，其中包含如下的 Markdown 文档。

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

实际粘贴到 Excel 的那一行是 **`### Headers (TSV)` 代码块中的那一行**。选项与输出格式的全部内容整理在 [4.1](../04-cli-tools/01-header-generator.md) 中。

还有一个 `all-header` 命令，可一次性提取文件夹内所有的 `[StaticDataRecord]` Record。各工作表的章节会在一个 Markdown 文件中依次整理。完整的命令形式与选项整理在 [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md) 中。

</br></br></br>

## 粘贴到 Excel

我们将上述 Markdown 文件中对应自己工作表的那一行应用到 Excel 表头。假设粘贴前的工作表中，已像 [3.1](./01-record-to-excel.md) 那样录入了数据作业者为便于辨认而写下的临时表头和数据。

|       | **A** | **B**   | **C**       | **D**       | **E**       | **F**       | **G**       | **H**       |
|-------|-------|---------|-------------|-------------|-------------|-------------|-------------|-------------|
| **1** | Id    | Name    | 数学科目     | 数学分数     | 英语科目     | 英语分数     | 科学科目     | 科学分数     |
| **2** | 1     | Alice   | Math        | 90          | English     | 85          | Science     | 88          |
| **3** | 2     | Bob     | Math        | 70          | English     | 95          | Science     | 75          |

1. 用 VS Code、记事本、浏览器渲染等任意方便的方式打开 `Student.md`。
2. 在该 Record 章节中，仅精确选中并复制 **`### Headers (TSV)` 下方代码块中的那一行** (不包含 ` ``` ` 标记行)。在该行上，通常 `Home → Shift+End → Ctrl+C` 是安全的。
3. 打开 Excel 的 `StudentReport.xlsx` 文件，转到 `Grades` 工作表。
4. 单击表头起始的单元格 (例如 `A1`)。必须是仅选中这一个单元格的状态。
5. 用 `Ctrl + V` 粘贴。

由于制表符分隔符会自然地一格一格进入不同的单元格，因此一次粘贴即可将第 1 行的临时表头替换为标准表头。

|       | **A** | **B**   | **C**                 | **D**               | **E**                 | **F**               | **G**                 | **H**               |
|-------|-------|---------|-----------------------|---------------------|-----------------------|---------------------|-----------------------|---------------------|
| **1** | Id    | Name    | Subjects[0].Subject   | Subjects[0].Score   | Subjects[1].Subject   | Subjects[1].Score   | Subjects[2].Subject   | Subjects[2].Score   |
| **2** | 1     | Alice   | Math                  | 90                  | English               | 85                  | Science               | 88                  |
| **3** | 2     | Bob     | Math                  | 70                  | English               | 95                  | Science               | 75                  |

临时表头 (`数学科目`、`数学分数` …) 与标准表头 (`Subjects[0].Subject` …) 之间的差异有多大，对比两个表的第 1 行便一目了然。数据行保持不变，仅有那一行表头发生了变化。

</br></br></br>

## 推荐的工作流程

即使在标准表头确定之前，也无需停止数据录入。如果将 **临时表头** 放在上一行，就可以并行推进 Record 定义和数据录入。

假设已就提取器选项 `--start-cell B3` 达成一致。此时工作表布局如下安排。

|       | **A**       | **B**       | **C**       | **D**       |
|-------|-------------|-------------|-------------|-------------|
| **1** | (自由)       | (自由)       | (自由)       | (自由)       |
| **2** |             | 临时表头     | 临时表头     | 临时表头     |
| **3** |             | 标准表头     | 标准表头     | 标准表头     |
| **4** |             | 数据         | 数据         | 数据         |

- `A` 列与第 `1`、`2` 行是提取器不会读取的自由区域。它们是写入诸如 **关于此表的说明、变更历史、负责人备注** 等仅在工作表内部有意义的信息的好位置。
- 在第 `B2` 行写下数据作业者易于辨认的临时名称 (例如“ID”“名称”“数学分数”)。
- 第 `B3` 行是标准表头的位置。在 Record 定义完成之前留空，之后将 `StaticDataHeaderGenerator` 结果 Markdown 的 `### Headers (TSV)` 代码块中的那一行原样粘贴进去。
- 从 `B4` 开始填充数据。

从数据作业者角度看的流程如下推进。

1. 与记录作业者就列布局、起始单元格 (`B3`) 达成一致。在此时点 Record `.cs` 即使尚未完成也无妨。
2. 在 `B2` 写下临时表头，并从 `B4` 开始录入数据。
3. 记录作业者确定 Record 后，运行 `StaticDataHeaderGenerator` 获取标准表头。
4. 将该结果粘贴到 `B3`。临时表头可以原样保留，也可以干净地删除。
5. 之后的提取使用 `ExcelColumnExtractor --start-cell B3` 进行 (推荐在 CI 中自动执行)。

</br></br></br>

## 全部选项与自动化

命令的两种形式 (`header` / `all-header`)、全部选项以及 bat 自动化示例整理在 [4.1 StaticDataHeaderGenerator](../04-cli-tools/01-header-generator.md) 中。

---

[← 上一篇: 3.1 处理 Excel](./01-record-to-excel.md) | [目录](../README.md) | [下一篇: 3.3 定义你的第一个 Record →](./03-first-record.md)
