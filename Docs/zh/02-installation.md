# 2. 安装

## 环境要求

- 需要 **.NET SDK** —— 确切版本在仓库的 `global.json` 中注明

</br></br></br>

## 安装方法

### 1. 从 GitHub Releases 获取

从 [Releases 页面](https://github.com/bluekms/StaticDataPipeline/releases) 下载单文件可执行程序，放到你想要的路径，或添加到 PATH。

|文件|用途|
|-|-|
|`ExcelColumnExtractor-v<版本>-win-x64.exe` / `-linux-x64`|Excel → CSV 抽取 CLI|
|`StaticDataHeaderGenerator-v<版本>-win-x64.exe` / `-linux-x64`|标准表头生成 CLI|
|`Sdp.dll`|运行时库 —— 从你的项目中引用|

两个 CLI 工具是 `--self-contained` 的单文件可执行程序，因此无需另行安装 .NET 运行时。`Sdp.dll` 目前必须从你的项目中直接引用（NuGet 包分发计划在将来支持）。

### 2. 从源代码构建

```bash
git clone https://github.com/bluekms/StaticDataPipeline.git
cd StaticDataPipeline
dotnet build -c Release
```

---

[← 上一篇: 1. 简介](./01-introduction.md) | [目录](./README.md) | [下一篇: 3.1 处理 Excel →](./03-usage/01-record-to-excel.md)
