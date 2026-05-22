# 2. Installation

## Requirements

- **.NET SDK** required — the exact version is specified in the repository's `global.json`

</br></br></br>

## How to Install

### 1. Download from GitHub Releases

From the [Releases page](https://github.com/bluekms/StaticDataPipeline/releases), download the single-file executables and place them in a path of your choice or add them to PATH.

|File|Purpose|
|-|-|
|`ExcelColumnExtractor-v<version>-win-x64.exe` / `-linux-x64`|Excel → CSV extraction CLI|
|`StaticDataHeaderGenerator-v<version>-win-x64.exe` / `-linux-x64`|Standard header generation CLI|
|`Sdp.dll`|Runtime library — referenced from your project|

The two CLI tools are `--self-contained` single-file executables, so no separate .NET runtime installation is required. `Sdp.dll` must currently be referenced directly from your project (NuGet package distribution is planned for the future).

### 2. Build from Source

```bash
git clone https://github.com/bluekms/StaticDataPipeline.git
cd StaticDataPipeline
dotnet build -c Release
```

---

[← Previous: 1. Introduction](./01-introduction.md) | [Table of Contents](./README.md) | [Next: 3.1 Working with Excel →](./03-usage/01-record-to-excel.md)
