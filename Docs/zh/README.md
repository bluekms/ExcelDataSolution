# Sdp 简体中文文档

这是流水线库 **StaticDataPipeline (Sdp)** 的简体中文文档，该库将 Excel 数据校验、加载为 C# 记录，并在内存中提供高速查询。

> 本文档由 AI 从作为权威来源的韩语原文机器翻译而成。

## 快速开始

如果你是初次接触，请从 **[快速开始](./quickstart.md)** 开始 —— 这是一页用 5 分钟完成 Record 定义 → 加载 → 查询的内容。Sdp 解决什么问题、以怎样的流程运行，在 [1. 简介](./01-introduction.md) 中讲述。

</br></br></br>

## 目录

### 1. [简介](./01-introduction.md)
Sdp 解决的问题、主要优点、数据流。

### 2. [安装](./02-installation.md)
环境要求与安装方法。

### 3. 使用法
通过示例学习的流水线。部分章节是 **数据作业者** 填写 Excel 时翻阅的指南，其余则是 **记录作业者** 组建 Record/Table/Manager 时翻阅的指南。数据结构达成一致后，两项工作可以互不等待、并行推进。
- [3.1 处理 Excel](./03-usage/01-record-to-excel.md) —— 数据作业者视角
- [3.2 标准表头生成器](./03-usage/02-header-generator.md) —— 数据作业者视角
- [3.3 定义你的第一个 Record](./03-usage/03-first-record.md) —— 记录作业者视角
- [3.4 实现 StaticDataTable](./03-usage/04-static-data-table.md)
- [3.5 用 StaticDataManager 管理多张表](./03-usage/05-static-data-manager.md)
- [3.6 外键 (ForeignKey, SwitchForeignKey)](./03-usage/06-foreign-keys.md)
- [3.7 StaticDataView 预生成视图](./03-usage/07-static-data-view.md)

### 4. CLI 工具
从构建流水线中调用的两个 CLI 工具的用法 —— 命令形态、全部选项、bat 示例。
- [4.1 StaticDataHeaderGenerator](./04-cli-tools/01-header-generator.md)
- [4.2 ExcelColumnExtractor](./04-cli-tools/02-column-extractor.md)

### 5. 高级功能
- [5.1 支持的类型 (Schemata)](./05-advanced/01-schemata.md)
- [5.2 Attribute 目录](./05-advanced/02-attributes.md)
- [5.3 类型品牌化模式](./05-advanced/03-type-branding.md)
- [5.4 校验概述](./05-advanced/04-validation.md)

### 6. [许可证](./06-license.md)

---

建议先用[快速开始](./quickstart.md)从头到尾跑一遍，然后在卡住的地方回到相应章节。如果你想通读，按照角色如下跟进会比较自然。

- **数据作业者**: 1 → 3.1 → 3.2 —— 编写 Excel 所需的内容，这两章就足够了。
- **记录作业者**: 1 → 3.3 直到最后 —— 最好把 Record/Table/Manager/View 的组建以及 CLI 工具（第 4 章）、高级功能（第 5 章）都通览一遍。
