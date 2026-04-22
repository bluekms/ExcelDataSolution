# 2. 설치

## 요구 환경

- **.NET SDK** 필요 — 정확한 버전은 리포지토리의 `global.json` 에 명시되어 있습니다

</br></br></br>

## 설치 방법

### 1. GitHub Releases에서 받기

[Releases 페이지](https://github.com/bluekms/StaticDataPipeline/releases) 에서 단일 실행 파일을 내려받아 원하는 경로에 배치하거나 PATH 에 추가합니다.

|파일|용도|
|-|-|
|`ExcelColumnExtractor-v<버전>-win-x64.exe` / `-linux-x64`|Excel → CSV 추출 CLI|
|`StaticDataHeaderGenerator-v<버전>-win-x64.exe` / `-linux-x64`|표준 헤더 생성 CLI|
|`Sdp.dll`|런타임 라이브러리 — 프로젝트에서 참조|

CLI 두 도구는 `--self-contained` 단일 실행 파일이라 .NET 런타임 설치가 따로 필요하지 않습니다. `Sdp.dll` 은 현재 프로젝트에서 직접 참조해야 합니다 (NuGet 패키지 배포는 추후 지원 예정).

### 2. 소스 빌드

```bash
git clone https://github.com/bluekms/StaticDataPipeline.git
cd StaticDataPipeline
dotnet build -c Release
```

---

[← 이전: 1. 소개](./01-introduction.md) | [목차](./README.md) | [다음: 3.1 Excel 작업하기 →](./03-usage/01-record-to-excel.md)
