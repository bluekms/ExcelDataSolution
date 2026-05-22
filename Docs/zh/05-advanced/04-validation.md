# 5.4 校验概述

本页用一页整理 Sdp 的校验在流水线的哪个时点运行。各个 Attribute 的详情请参阅 [5.2 Attribute 目录](./02-attributes.md)，整体数据流请参阅 [1. 简介](../01-introduction.md)。

## 三个校验时点

|时点|执行主体|对象|
|-|-|-|
|**声明检查**|`ExcelColumnExtractor` · `StaticDataHeaderGenerator`|Record/Attribute **声明** 本身的缺陷|
|**提取**|`ExcelColumnExtractor`|Excel **单元格值** 是否与架构和 Attribute 兼容|
|**加载**|`StaticDataManager.LoadAsync`|运行时加载时点的结构、值与引用检查|

声明检查和提取在构建流水线（离线）中运行，加载在应用程序运行时运行。由于声明检查分析 `.cs` 源代码，因此在 `ExcelColumnExtractor` 和 `StaticDataHeaderGenerator` 任一运行时都会执行，而在没有源代码的运行时则不会运行。

</br></br></br>

## 各检查项的运行时点

`O` 表示在该时点执行检查，`—` 表示不执行。

### A. 类型与结构架构

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|类型是否受支持|O|—|—|拒绝不受支持的类型|
|集合自身禁止 nullable|O|—|—|`ImmutableArray<T>?`·`FrozenSet<T>?`·`FrozenDictionary<,>?`|
|禁止 Nullable 的 Record 元素|O|—|—|Record 数组/集合/Map Value 的 nullable|
|禁止循环引用|O|—|—|Record 直接或间接包含自身|
|Map Key non-nullable|O|—|—||
|Map Key↔Value `[Key]` 类型一致|O|—|—||

### B. Attribute 一致性

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|`[Length]` 必需|O|—|—|多列集合|
|`[NullString]` 必需 / 误用|O|—|—|nullable 必需，non-nullable 禁止|
|`[DateTimeFormat]` 必需 / 误用|O|—|—|`DateTime` 必需，非 DateTime 禁止|
|`[TimeSpanFormat]` 必需 / 误用|O|—|—|`TimeSpan` 必需，非 TimeSpan 禁止|
|`[RegularExpression]` 类型|O|—|—|仅限 `string` / `string?`|
|`[Range]` 可应用的类型|O|—|—|禁止 `bool`·集合·record|
|`[CountRange]` 一致性|O|—|—|需要 `[SingleColumnCollection]`，与 `[Length]` 互斥，minCount>0|
|`[SingleColumnCollection]` 一致性|O|—|—|不允许用于 Map，元素仅限 primitive|
|`[Key]` 一致性|O|—|O|每个 Record 最多 1 个·non-nullable 为声明检查 / Map Value 中 `[Key]` 的存在为声明检查与加载两端|
|`[StaticDataRecord]` 的存在|O|O|O|识别声明检查对象 / 提取对象为 0 个时终止 / 加载时必需|

### C. 单元格值

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|表头存在|—|O|O||
|单元格值与类型的兼容性|—|O|O|加载通过转换（`Convert.ChangeType`）失败来检出|
|enum 成员名的有效性|—|O|O|加载使用 `Enum.IsDefined` —— `[Key]` enum 省略|
|`DateTime`/`TimeSpan` 格式一致|—|O|O|两端以相同 format 进行 `ParseExact`|
|`[Range]` 值范围|—|O|O||
|`[RegularExpression]` 模式匹配|—|O|O||
|`[CountRange]` 拆分元素个数|—|O|O|单列集合|
|Primary Key 在工作表内重复|—|O|—|`[Key]` 列。加载没有自动检查 —— 由 `UniqueIndex` 以 opt-in 方式保证|

### D. 外键

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|禁止同时附加 `[ForeignKey]`·`[SwitchForeignKey]`|O|—|O|两端检查|
|禁止重复的 `[SwitchForeignKey]` 条件|O|—|O|两端检查|
|FK 目标 TableSet 存在|—|—|O|名称拼写错误由加载前的类型检查检出，通过 `disabledTables` 移除的表的引用由加载后的引用校验检出（两者均为 `FkTargetNotFound`）|
|FK 目标不是 `[SingleColumnCollection]`|—|—|O||
|FK 目标列存在|—|—|O||
|`[SwitchForeignKey]` 条件列存在|—|—|O||
|`[SwitchForeignKey]` 条件值分支匹配|—|—|O||
|FK 引用值存在|—|—|O|实际的引用完整性|

### E. 加载结构

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|TableSet 单一构造函数|—|—|O||
|表参数类型|—|—|O|是否为 `StaticDataTable<,>`|
|表构造函数（`ImmutableArray`）存在|—|—|O||
|ViewSet 单一构造函数|—|—|O||
|View 参数类型与 non-nullable|—|—|O||
|View 构造函数（`TableSet`）存在|—|—|O||
|禁止并发进入 `LoadAsync`|—|—|O||
|`UniqueIndex` 键重复|—|—|O|在表/视图创建时，opt-in|

### F. 用户自定义校验

|检查项|声明检查|提取|加载|备注|
|-|-|-|-|-|
|表自校验|—|—|O|`StaticDataTable.Validate()` override，表实例化后立即执行|
|管理器交叉校验|—|—|O|`StaticDataManager.Validate(TTableSet)` override，在所有 FK 校验之后|
|视图自校验|—|—|O|`StaticDataView.Validate()` override，视图构建后立即执行|

</br></br></br>

## 只有经过提取的 CSV 才上线运营

在单元格值检查（C）中，`[Range]`·`[RegularExpression]`·`[CountRange]` 在提取和加载两端以相同规则运行。但 Primary Key 重复以及声明一致性检查（A·B）仅在提取阶段（含声明检查）执行。如果将未经提取阶段的 CSV 直接投入运行时，这些检查会被跳过，因此健康的流水线始终只将经过提取器的 CSV 上线运营。

---

[← 上一篇: 5.3 类型品牌化模式](./03-type-branding.md) | [目录](../README.md) | [下一篇: 6. 许可证 →](../06-license.md)
