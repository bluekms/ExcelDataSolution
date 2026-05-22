# 4.1 StaticDataHeaderGenerator

`StaticDataHeaderGenerator` 是从 C# Record 定义中提取 **标准表头** 的 CLI 工具。它让你无需手动对齐表头，即可自动填充表头行 —— 即使表头很长（例如对象数组的情况）也是如此。

结果以 **Markdown 文档** 的形式生成。在一个文件中，按 Record 为单位创建小节，每个小节中同时包含表头列表 (List) 和用分隔符连接的表头行 (Code block)。数据作业者从 Code block 中复制一行，粘贴到 Excel 表头中（参见 [3.2 标准表头生成器](../03-usage/02-header-generator.md)）。

本章聚焦于工具本身的用法 —— 命令形式、全部选项、输出格式以及 bat 示例。

</br></br></br>

## 命令结构

命令有两种形式，由第一个参数指定使用哪一种形式。

```bash
StaticDataHeaderGenerator.exe header [选项...]
StaticDataHeaderGenerator.exe all-header [选项...]
```

- `header` —— 生成 **单个 Record** 的标准表头。必须用 `--record-name` 指定目标。
- `all-header` —— 一次性生成 `--record-path` 文件夹下所有 `[StaticDataRecord]` Record 的标准表头。

`header` 将生成的 Markdown 输出到控制台，如果指定了 `--output-file`，也会保存到该文件（控制台输出仍保留）。`all-header` 处理整个文件夹，输出量可能很大，因此不会输出到控制台，只保存到 `--output-file` 指定的文件。

</br></br></br>

## 选项

### `header` —— 单个 Record

|选项|含义|默认值|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` 文件或目录的路径|必填|
|`-n`, `--record-name`|目标 Record 名称（类名，例如 `StudentRecord`）|必填|
|`-s`, `--separator`|Code block 中表头之间插入的分隔符|`\t`（制表符）|
|`-o`, `--output-file`|输出文件路径（不指定则输出到控制台）|无|
|`-l`, `--log-path`|日志目录路径（其下会生成按日期划分的 `log<日期>.txt`）|无|
|`-m`, `--min-log-level`|最低日志级别（Verbose, Debug, Information, Warning, Error, Fatal）|Information|

### `all-header` —— 整个文件夹

|选项|含义|默认值|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` 文件或目录的路径|必填|
|`-s`, `--separator`|Code block 中表头之间插入的分隔符|`\t`（制表符）|
|`-o`, `--output-file`|输出文件路径（`all-header` 没有控制台输出，省略则结果不会保留为文件）|无|
|`-l`, `--log-path`|日志目录路径（其下会生成按日期划分的 `log<日期>.txt`）|无|
|`-m`, `--min-log-level`|最低日志级别|Information|

`all-header` 没有 `--record-name`。因为它处理整个文件夹，所以无需指定目标。

如果给 `--output-file` 只提供不带扩展名的路径，会自动追加 `.md`。即使指定了扩展名，输出内容也始终是 Markdown。

</br></br></br>

## 输出格式

结果是具有如下结构的 Markdown 文档。

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

- 最顶部为 `# StaticDataHeaderGenerator Results` 一行。
- 每个 Record 对应一个 `## {RecordFullName}` 小节。如果 Record 声明在某个命名空间内，`{RecordFullName}` 会采用 `命名空间.类型名` 的形式（本文档的示例 Record 假定为不带命名空间定义，因此以简单名称表示）。
  - `Excel File`、`Sheet Name` —— `[StaticDataRecord]` 的两个参数。
  - `### Headers (List)` —— 每行一个表头，以 bullet 列出。
  - `### Headers (TSV)` —— 用 `--separator` 连接的一行，标注在代码块中。
  
`--separator` 影响的位置仅为 **`### Headers` 小节的标签及其下方代码块中的那一行**。标签的确定规则为：制表符为 `(TSV)`，逗号为 `(CSV)`，其他情况则为不带括号的 `### Headers`。表头列表 (List) 和其他元信息保持不变。

</br></br></br>

## 运行示例

本节假定 `./Records` 文件夹中有以下两个 Record。

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

### 单个 Record 的表头 —— 文件输出

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --output-file ./Headers/Student.md
```

生成的 `./Headers/Student.md`：

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

复制 `### Headers (TSV)` 代码块中的那一行，粘贴到 Excel 表头的第一个单元格，它会自动展开（[3.2 —— 粘贴到 Excel](../03-usage/02-header-generator.md#粘贴到-excel)）。

</br></br>

### 单个 Record 的表头 —— 控制台输出

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord
```

省略 `--output-file` 时，上述 Markdown 文档仅输出到控制台。

</br></br>

### 整个文件夹 —— 输出为一个文件

```bash
StaticDataHeaderGenerator.exe all-header ^
  --record-path ./Records ^
  --output-file ./Headers/AllHeaders.md
```

`./Records` 下所有 `[StaticDataRecord]` Record 会被整理到一个 Markdown 文件中，按工作表分小节。生成的 `./Headers/AllHeaders.md`：

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

数据作业者找到对应自己工作表的小节，复制 `### Headers (TSV)` 中的那一行，粘贴到 Excel 中。

</br></br>

### 更改分隔符

默认分隔符是制表符，但可以改成逗号或其他字符。

```bash
StaticDataHeaderGenerator.exe header ^
  --record-path ./Records ^
  --record-name StudentRecord ^
  --separator , ^
  --output-file ./Headers/Student.md
```

如果对比用两种不同分隔符提取同一个 `StudentRecord` 的结果，只有 **`### Headers` 小节的标签及其下方代码块中的那一行** 会变化。

制表符分隔（省略 `--separator`，默认）：

````markdown
### Headers (TSV)
```
Id	Name	Subjects[0].Subject	Subjects[0].Score	...
```
````

逗号分隔（`--separator ,`）：

````markdown
### Headers (CSV)
```
Id,Name,Subjects[0].Subject,Subjects[0].Score,...
```
````

文档的其余部分（标题、`Excel File`、`Sheet Name`、`Headers (List)`）保持不变。如果使用制表符或逗号以外的分隔符，标签会写为 `### Headers`（不带括号）。

粘贴到 Excel 时，**制表符分隔最为方便** —— 粘贴到一个单元格后会自动向相邻单元格展开。逗号等其他分隔符可能还需要 Excel 的「分列」之类的转换步骤。

</br></br>

#### 如果表头中含有分隔符则会阻止生成

如果所选分隔符原样出现在某个表头名称中（例如 `--separator ,` 而生成了 `Sub,Total` 这样的表头），粘贴后列会被错误地拆分，破坏数据一致性。表头生成器检测到此类冲突时，会以 `InvalidOperationException` 立即中断，并在消息中包含冲突表头的列表进行报告。通常只要整理 record 参数名或 `[ColumnName]` 的值，使其不含分隔符即可解决。

</br></br></br>

## 整理成 bat 文件

为了不必每次都记住并输入选项，把 **一个 bat 文件** 放在 Record 文件夹旁边会很方便。

```bat
@echo off
StaticDataHeaderGenerator.exe all-header ^
  --record-path .\Records ^
  --output-file .\Headers\AllHeaders.md
pause
```

有了 `pause`，确认结果消息后窗口才会关闭，因此可以放心地双击运行。结果文件是汇集了所有工作表表头小节的 Markdown 文档。

bat 文件在本地很方便，但如果把同样的命令注册为 GitHub Actions 之类 CI 的一个步骤，就能直接集成到构建流程中。

</br></br></br>

## 推荐工作流程

1. 当 Record `.cs` 大致确定后，用 `all-header` 把全部表头提取到一个 Markdown 文件中。
2. 数据作业者从该文件中找到对应自己工作表的小节，复制 `### Headers (TSV)` 中的那一行，粘贴到 Excel 中（[3.2](../03-usage/02-header-generator.md#粘贴到-excel)）。
3. 每当 Record 变化时，用 bat 重新提取即可 —— 会在同一位置生成新的 Markdown 文件，数据作业者在同一位置获得更新后的表头。

如果在 CI 环境中运行表头生成器并把生成的 Markdown 文件作为成果物上传，数据作业者就总能在同一位置获得最新版本。

---

[← 上一篇: 3.7 StaticDataView 预生成视图](../03-usage/07-static-data-view.md) | [目录](../README.md) | [下一篇: 4.2 ExcelColumnExtractor →](./02-column-extractor.md)
