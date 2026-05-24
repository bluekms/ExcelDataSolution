# 3.3 定义你的第一个 Record

> 从这里开始，视角切换为 **记录作业者** 的视角。从数据作业者视角进行的 Excel 编写已在 [3.1](./01-record-to-excel.md)、[3.2](./02-header-generator.md) 中介绍，从本章起将看 C# 端如何编写 Record、Table 和 Manager。

这是一个工作表已经填好、你首次为其编写匹配的 C# Record 的场景。假设示例工作表如下。

|       | **A**  | **B**    | **C**       | **D**   | **E**        |
|-------|--------|----------|-------------|---------|--------------|
| **1** | Id     | Name     | Memo        | Cost    | Category     |
| **2** | 1      | Potion   | 恢复物品     | 100     | Consumable   |
| **3** | 2      | Sword    | 基础剑       | 5000    | Weapon       |
| **4** | 3      | Shield   | 基础盾       | 4000    | Armor        |

`Memo` 是数据作业者参考用的列。C# 端不会使用它。**Record 不要求的列不会被提取到 CSV** — 请先注意下面的结果 CSV 中 `Memo` 缺失这一点。

假设数据作业者将价格称作 `Cost`，但你在 C# 代码中想使用 `Price` 这个名字。这种情况下可以用 `[ColumnName]` 将工作表表头与参数名分开映射。

## Record 定义

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

虽然简短，但包含了所有必要的信息。我们逐个来看。

### `[StaticDataRecord("GameItems", "Items")]`

它指定此 Record 对应哪个 Excel 文件的哪个工作表。第一个参数是 **Excel 文件名** (不含扩展名)，第二个参数是 **工作表名**。它用于两个用途。

- `ExcelColumnExtractor` 提取 CSV 时，用于找到目标文件和工作表。
- 用于提取结果 CSV 的文件名 — `{文件}.{工作表}.csv`。在上述示例中为 `GameItems.Items.csv`。

### `int Id`、`string Name`

如果没有特殊的 Attribute，列名与 **参数名相同**。工作表的表头必须有 `Id`、`Name` 列才能进行映射。

### `[ColumnName("Cost")][Range(0, 1_000_000)] int Price`

`[ColumnName(name)]` 用于在 Excel 表头名与 C# 参数名不同时告知其映射关系。上述工作表的表头是 `Cost`，而 Record 参数是 `Price`，因此用 `[ColumnName("Cost")]` 将两者连接起来。如果表头与参数名相同，则无需特意书写。

`[Range(min, max)]` 检查值是否在指定范围之内。它是继承自 `System.ComponentModel.DataAnnotations.RangeAttribute` 的 Attribute。超出范围的值会在提取阶段和运行时加载两端都被过滤掉。

> `1_000_000` 是 C# 的 [数字字面量分隔符](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types#integer-literals) 写法，与 `1000000` 是相同的值。它只是辅助可读性，因此写成 `[Range(0, 1000000)]` 也可以。

### `ItemCategory Category`

`enum` 是 **按字符串匹配** 的。CSV 单元格中必须写有 `Consumable` 才会被解析为 `ItemCategory.Consumable`。它不是整数值，而且大小写也必须完全一致 (`consumable`、`CONSUMABLE` 会失败)。未定义的名称同样会导致加载失败。

</br></br></br>

## 运行提取

Record 定义完成后，用 **`ExcelColumnExtractor`** 从工作表中提取 CSV。提取器只需指定 Record 文件夹、Excel 文件夹、输出文件夹三处，就会自动匹配并处理其中所有的 Record/Excel。

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

- `--record-path` — 含有附加了 `[StaticDataRecord]` 的 Record `.cs` 文件的文件夹
- `--excel-path` — 含有 Excel 文件的文件夹
- `--output-path` — 将创建结果 CSV 的文件夹

如果起始单元格位置不是 `A1`，用 `--start-cell` 告知。诸如按构建版本分离产物 (`--version`)、更改编码 (`--encoding`)、日志设置等全部选项整理在 [4.2 ExcelColumnExtractor](../04-cli-tools/02-column-extractor.md) 中。

</br></br>

### 结果 CSV

仅筛选出上述 Record 所要求的列，生成 `GameItems.Items.csv`。

```
Id,Name,Cost,Category
1,Potion,100,Consumable
2,Sword,5000,Weapon
3,Shield,4000,Armor
```

CSV 文件名遵循 **`{文件}.{工作表}.csv`** 规则。`GameItems.xlsx` 的 `Items` 工作表 → `GameItems.Items.csv`。

CSV 的表头原样保留工作表的原始表头 (`Cost`)。在加载阶段，`[ColumnName("Cost")]` 会将 `Cost` 列连接到 Record 的 `Price` 参数。

原始工作表中的 `Memo` 因 Record 不要求而不包含在 CSV 中。服务器、客户端和工具能各自用不同的 Record 定义消费同一份 Excel，原因正在于此。

</br></br>

### 提取阶段验证的内容

提取器不只是简单地把单元格抄写过去，还会一并检查以下内容。

- **Record 端架构本身的缺陷** — 提取器用 Roslyn 解析 `.cs` 文件，捕获错误的 Attribute 用法等 (在提取器执行时运作，而非作为 IDE 分析器)。
- **Record 所要求的列是否存在于工作表中** — 如果缺失则失败，并报告是哪个工作表的哪一列。
- **单元格值是否与类型兼容** — 数值列中混入字符、超出固定长度的集合、`[Range]` / `[RegularExpression]` / 格式违规等都会在提取时点检查。
- **Primary Key 重复** — 如果附加了 `[Key]` 的列的值在工作表内重复，则失败。`[Key]` 并非必需，没有 PK 的数据表也是允许的 (这种情况下重复检查本身会被跳过)。

外键 (`[ForeignKey]`、`[SwitchForeignKey]`) 的完整性不在提取阶段验证，而是在运行时的 `LoadAsync` 中验证 ([3.6](./06-foreign-keys.md))。

</br></br></br>

## 推荐的工作流程

1. 与数据作业者就 Record `.cs` 与 Excel 工作表的列布局、起始单元格位置达成一致。
2. 在构建流水线 (或本地 bat) 中将 `ExcelColumnExtractor` 调用作为一个步骤。每当 Record 变更时，只需运行这个步骤即可。
3. 产出的 CSV 会被复制到运行时构建输出，由 `StaticDataManager.LoadAsync` 读取 ([3.5](./05-static-data-manager.md))。

如果将提取器调用作为 CI 步骤，那么诸如表头缺失或类型不匹配这类在提取阶段被过滤掉的错误，无需有人每次手动运行，就会在合并前自动暴露出来。

</br></br></br>

## 下一步

- 要实际加载到内存并进行查询，需创建 **StaticDataTable**。这将在 [3.4](./04-static-data-table.md) 中介绍。
- 可用类型的完整列表以及每种类型必须随附的 Attribute 整理在 [5.1 支持的类型](../05-advanced/01-schemata.md) 中。
- Attribute 目录汇集在 [5.2](../05-advanced/02-attributes.md) 中。

---

[← 上一篇: 3.2 标准表头生成器](./02-header-generator.md) | [目录](../README.md) | [下一篇: 3.4 实现 StaticDataTable →](./04-static-data-table.md)
