# 4.2 ExcelColumnExtractor

`ExcelColumnExtractor` 是接收 Excel 文件群和 C# Record 定义作为输入，仅挑出各 Record 所需的列并导出为 CSV 的 CLI 工具。它在构建步骤中运行一次，Sdp 运行时只读取那些 CSV。

从 Record 作业者的视角看提取器在何处登场，在 [3.3 定义你的第一个 Record](../03-usage/03-first-record.md#执行提取) 中介绍。本章聚焦于工具本身的用法 —— 命令形式、全部选项、输出格式以及 bat 示例。

</br></br></br>

## 命令结构

`ExcelColumnExtractor` 是单一命令。没有子命令（verb）。

```bash
ExcelColumnExtractor.exe [选项...]
```

用三个必填选项指定输入文件夹、Excel 文件夹和输出文件夹。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

</br></br></br>

## 选项

|选项|含义|默认值|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` 文件或目录的路径|必填|
|`-e`, `--excel-path`|包含 Excel 文件的目录路径|必填|
|`-o`, `--output-path`|CSV 输出目录的路径|必填|
|`-s`, `--start-cell`|表头起始单元格地址（例如 `A1`、`B3`、`C7`）|`A1`|
|`-v`, `--version`|输出版本 —— 指定后会输出到 `output-path/version` 子文件夹|无|
|`-f`, `--force`|使用 `--version` 时，即使该文件夹中已有文件也覆盖|`false`|
|`-c`, `--encoding`|输出 CSV 的编码（UTF-8 不带 BOM；UTF-16、UTF-32、ASCII 等）|`UTF-8`|
|`-l`, `--log-path`|日志目录路径（其下会生成按日期划分的 `log<日期>.txt`）|无|
|`-m`, `--min-log-level`|最低日志级别（Verbose, Debug, Information, Warning, Error, Fatal）|Information|

</br></br></br>

## 输出格式

提取结果的 CSV 按 **`{文件}.{工作表}.csv`** 的规则生成。

| Excel 文件 | 工作表 | 输出 CSV |
|-|-|-|
| `GameItems.xlsx` | `Items` | `GameItems.Items.csv` |
| `Heroes.xlsx` | `BaseStats` | `Heroes.BaseStats.csv` |

CSV 的表头原样保留工作表的 **原始表头**。即使 Record 一侧用 `[ColumnName("Cost")]` 映射到了不同的参数名，CSV 中仍是工作表的 `Cost`。映射在加载步骤中处理。

Record 不需要的列不会包含在 CSV 中。同一份 Excel 之所以能被服务端、客户端、工具各自以不同的 Record 定义来消费，原因正在于此。

</br></br></br>

## 表头起始单元格 (`--start-cell`)

`--start-cell` 告诉提取器各工作表中 **表头第一格在何处**。从该单元格的下一行起视为数据。

|       | **A**             | **B**    | **C**     | **D**   | **E**        |
|-------|-------------------|----------|-----------|---------|--------------|
| **1** | 道具表             |          |           |         |              |
| **2** | 最后修改 2026-05-15 |        |           |         |              |
| **3** | Id                | Name     | Memo      | Price   | Category     |
| **4** | 1                 | Potion   | 回复道具   | 100     | Consumable   |

上面的工作表用 `--start-cell A3` 提取。第 `1`、`2` 行是自由区域（工作表标题、变更历史等），会被忽略。

省略该选项时，假定从 `A1` 开始。在一个项目内，把起始单元格商定为统一的一个会更简单。

</br></br>

### 按 Record 为单位覆盖起始单元格

如果大多数工作表从相同位置开始，但只有部分工作表需要从其他位置开始，就在 `[StaticDataRecord]` 的第三个参数中写起始单元格。该值存在时，优先于 `--start-cell` CLI 选项（参见 [5.2 `[StaticDataRecord]`](../05-advanced/02-attributes.md#attr-staticdatarecord)）。

```csharp
// 项目默认商定为 B3，但唯独这个工作表从 A1 开始
[StaticDataRecord("GameItems", "Quests", "A1")]
public sealed record QuestRecord(int Id, string Title);
```

把 CLI 调用统一为一行，仅用 attribute 标注例外，这种方式在工作表数量增加时便于管理。

</br></br></br>

## 版本文件夹 (`--version`, `--force`)

指定 `--version` 后，输出会汇集到 `output-path/<version>/` 子文件夹中。当想按构建号或数据补丁号分离成果物时使用。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3
```

结果：

```
Csv/
└── 1.2.3/
    ├── GameItems.Items.csv
    ├── Heroes.BaseStats.csv
    └── ...
```

</br></br>

### 确定版本字符串时需注意的要点

如果同一版本文件夹中已有文件，提取会 **以错误中断**（防止意外覆盖）。因此，版本字符串必须是 **一旦生成就不会再产生相同值的标识符**。

仅使用日期的标识符（`2026-05-18`）不合适，因为在同一天多次提取的流程中每次都会冲突。推荐的标识符如下。

- **SemVer + 构建元数据** —— `1.2.3-build.42`、`1.2.3+commit.a1b2c3d`
- **CI 构建号** —— `$(Build.BuildNumber)`、`${{ github.run_number }}` 等 CI 在每次构建时递增的值
- **日期 + 构建计数器** —— `2026-05-18.42`（同一天的第 N 次构建）
- **提交哈希** —— `a1b2c3d`（按 PR/合并为单位保存成果物时）

如果有意需要重新提取到同一版本文件夹（例如出于调试目的重新生成同一构建），请添加 `--force`。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3 ^
  --force
```

如果不指定 `--version`，冲突检查不会运行，因此 `--force` 也没有意义 —— 输出直接进入 `output-path`，同名文件会被直接覆盖。通常的做法是：本地开发不带 `--version` 运行，CI/发布成果物则用 `--version` 分离。

</br></br></br>

## 编码 (`--encoding`)

默认值是不带 BOM 的 **UTF-8**。大多数情况下保持原样即可。如果某些消费者要求 UTF-16 或其他编码，则进行指定。

支持的编码：

|值|含义|
|-|-|
|`UTF-8`|不带 BOM 的 UTF-8（默认）|
|`UTF-16`|UTF-16 LE|
|`UTF-32`|UTF-32|
|`ASCII`|ASCII|
|其他|通过 .NET `Encoding.GetEncoding(name)` 处理。例如 `EUC-KR`、`Windows-1252`|

</br></br></br>

## 运行示例

### 基本

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

### 起始单元格商定为 `B3` 的项目

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3
```

### 按构建版本分离成果物

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3 ^
  --version 1.2.3-build.42
```

### 将日志详细地记录到文件

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --log-path ./Logs ^
  --min-log-level Debug
```

</br></br></br>

## 整理成 bat 文件

由于提取器在构建步骤中经常被调用，整理成 bat 文件会很方便。

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

提取失败时退出码非零，因此可以基于 `errorlevel` 进行分支。

如果把同样的命令注册为 GitHub Actions 之类 CI 的一个步骤，就能直接集成到构建流程中。提取失败时退出码非零，因此会直接导致工作流失败，使错误数据在被合并之前暴露出来。

</br></br></br>

## 推荐工作流程

1. 在一个项目内把 `--start-cell` 的位置商定为统一的一个（例如 `B3` —— `A` 列和第 `1`、`2` 行是工作表的自由区域）。
2. 在构建流水线中把提取器的调用作为一个步骤。
3. 生成的 CSV 会被复制到运行时构建输出文件夹，由 `StaticDataManager.LoadAsync` 读取（参见 [3.5](../03-usage/05-static-data-manager.md)）。
4. 如果想把构建版本记录到数据中，用 `--version` 分离输出文件夹。

提取本身过滤出来的检验有以下四项。

- **Record 架构缺陷** —— 提取器用 Roslyn 解析 `.cs` 文件，捕获错误的 Attribute 用法、不支持的类型等（在提取器执行时运行，而非 IDE 构建时）。
- **表头缺失** —— Record 要求的列在工作表中不存在时。
- **单元格值与类型的兼容性** —— 单元格值与 Record 的类型/Attribute 不符时，例如 `[Range]`、`[RegularExpression]`、`[DateTimeFormat]`、`[Length]`、`[CountRange]`、enum 成员等。
- **Primary Key 重复** —— 标注了 `[Key]` 的列在工作表内的值重复。

外键（`[ForeignKey]`、`[SwitchForeignKey]`）检验发生在运行时（`LoadAsync`），而非提取步骤（参见 [3.6](../03-usage/06-foreign-keys.md)）。

---

[← 上一篇: 4.1 StaticDataHeaderGenerator](./01-header-generator.md) | [目录](../README.md) | [下一篇: 5.1 支持的类型 (Schemata) →](../05-advanced/01-schemata.md)
