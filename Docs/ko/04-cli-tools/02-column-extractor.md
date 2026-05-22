# 4.2 ExcelColumnExtractor

`ExcelColumnExtractor` 는 Excel 파일들과 C# Record 정의를 입력받아, 각 Record 가 요구하는 컬럼만 추려 CSV 로 내보내는 CLI 도구입니다. 빌드 단계에서 한 번 돌리고, Sdp 런타임은 그 CSV 만 읽습니다.

레코드 작업자 시점에서 추출기가 어디에서 등장하는지는 [3.3 첫 Record 정의하기](../03-usage/03-first-record.md#추출-실행하기) 에서 다룹니다. 이 챕터는 도구 자체의 사용법 — 명령 형태, 옵션 전체, 출력 형식, bat 예제 — 에 집중합니다.

</br></br></br>

## 명령 구조

`ExcelColumnExtractor` 는 단일 명령입니다. 서브 명령 (verb) 이 없습니다.

```bash
ExcelColumnExtractor.exe [옵션...]
```

필수 옵션 세 개로 입력 폴더, Excel 폴더, 출력 폴더를 지정합니다.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

</br></br></br>

## 옵션

|옵션|의미|기본값|
|-|-|-|
|`-r`, `--record-path`|Record `.cs` 파일 또는 디렉터리 경로|필수|
|`-e`, `--excel-path`|Excel 파일이 있는 디렉터리 경로|필수|
|`-o`, `--output-path`|CSV 출력 디렉터리 경로|필수|
|`-s`, `--start-cell`|헤더 시작 셀 주소 (예: `A1`, `B3`, `C7`)|`A1`|
|`-v`, `--version`|출력 버전 — 지정하면 `output-path/version` 하위 폴더에 산출|없음|
|`-f`, `--force`|`--version` 사용 시 해당 폴더에 파일이 이미 있어도 덮어씀|`false`|
|`-c`, `--encoding`|출력 CSV 인코딩 (UTF-8 은 BOM 없음, UTF-16, UTF-32, ASCII 등)|`UTF-8`|
|`-l`, `--log-path`|로그 디렉터리 경로 (그 아래 일자별 `log<날짜>.txt` 가 생성됨)|없음|
|`-m`, `--min-log-level`|최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)|Information|

</br></br></br>

## 출력 형식

추출 결과 CSV 는 **`{파일}.{시트}.csv`** 규칙으로 만들어집니다.

| Excel 파일 | 시트 | 출력 CSV |
|-|-|-|
| `GameItems.xlsx` | `Items` | `GameItems.Items.csv` |
| `Heroes.xlsx` | `BaseStats` | `Heroes.BaseStats.csv` |

CSV 의 헤더는 시트의 **원본 헤더** 를 그대로 유지합니다. Record 측에서 `[ColumnName("Cost")]` 로 다른 파라미터 이름에 매핑하더라도 CSV 에는 시트의 `Cost` 가 들어갑니다. 매핑은 로드 단계에서 처리됩니다.

Record 가 요구하지 않은 컬럼은 CSV 에 포함되지 않습니다. 같은 Excel 을 서버, 클라이언트, 툴이 각자 다른 Record 정의로 소비할 수 있는 이유가 여기에 있습니다.

</br></br></br>

## 헤더 시작 셀 (`--start-cell`)

`--start-cell` 은 각 시트에서 **헤더 첫 칸이 어디인지** 를 알려줍니다. 그 셀의 다음 행부터 데이터로 간주합니다.

|       | **A**             | **B**    | **C**     | **D**   | **E**        |
|-------|-------------------|----------|-----------|---------|--------------|
| **1** | 아이템 테이블       |          |           |         |              |
| **2** | 최종 수정 2026-05-15 |        |           |         |              |
| **3** | Id                | Name     | Memo      | Price   | Category     |
| **4** | 1                 | Potion   | 회복 아이템 | 100     | Consumable   |

위 시트는 `--start-cell A3` 으로 추출합니다. `1`, `2` 행은 자유 영역 (시트 제목, 변경 이력 등) 이라 무시됩니다.

옵션을 생략하면 `A1` 에서 시작한다고 가정합니다. 한 프로젝트 안에서는 시작 셀을 하나로 합의해 두는 편이 단순합니다.

</br></br>

### Record 단위로 시작 셀 덮어쓰기

대부분의 시트는 같은 위치에서 시작하지만 일부 시트만 다른 위치에서 시작해야 한다면, `[StaticDataRecord]` 의 세 번째 인자에 시작 셀을 적습니다. 이 값이 있으면 `--start-cell` CLI 옵션보다 우선합니다 ([5.2 `[StaticDataRecord]`](../05-advanced/02-attributes.md#attr-staticdatarecord) 참고).

```csharp
// 프로젝트 기본은 B3 으로 합의되어 있지만, 이 시트만 A1 에서 시작
[StaticDataRecord("GameItems", "Quests", "A1")]
public sealed record QuestRecord(int Id, string Title);
```

CLI 호출은 한 줄로 통일해 두고, 예외만 attribute 로 표시하는 방식이 시트 수가 늘어났을 때 관리하기 편합니다.

</br></br></br>

## 버전 폴더 (`--version`, `--force`)

`--version` 을 지정하면 출력이 `output-path/<version>/` 하위 폴더에 모입니다. 빌드 번호나 데이터 패치 번호로 산출물을 분리해 두고 싶을 때 씁니다.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3
```

결과:

```
Csv/
└── 1.2.3/
    ├── GameItems.Items.csv
    ├── Heroes.BaseStats.csv
    └── ...
```

</br></br>

### 버전 문자열을 정할 때 주의할 점

같은 버전 폴더에 이미 파일이 있으면 추출은 **에러로 중단** 됩니다 (의도치 않은 덮어쓰기 방지). 따라서 버전 문자열은 **한 번 만들어지면 다시 같은 값이 나오지 않는 식별자** 여야 합니다.

날짜만 쓰는 식별자 (`2026-05-18`) 는 같은 날 여러 번 추출하는 흐름에서 매번 충돌하므로 적합하지 않습니다. 권장되는 식별자는 다음과 같습니다.

- **SemVer + 빌드 메타데이터** — `1.2.3-build.42`, `1.2.3+commit.a1b2c3d`
- **CI 빌드 번호** — `$(Build.BuildNumber)`, `${{ github.run_number }}` 등 CI 가 매 빌드마다 증가시키는 값
- **날짜 + 빌드 카운터** — `2026-05-18.42` (같은 날의 N 번째 빌드)
- **커밋 해시** — `a1b2c3d` (PR/머지 단위로 산출물을 보관할 때)

같은 버전 폴더에 의도적으로 다시 추출해야 한다면 (예: 디버깅 목적으로 같은 빌드를 재생성) `--force` 를 추가합니다.

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --version 1.2.3 ^
  --force
```

`--version` 을 지정하지 않으면 충돌 검사가 동작하지 않으므로 `--force` 도 의미가 없습니다 — 곧바로 `output-path` 에 산출하며 같은 이름의 파일은 그대로 덮어씁니다. 로컬 개발에서는 `--version` 없이 돌리고, CI/배포 산출물에서는 `--version` 으로 분리하는 식이 일반적입니다.

</br></br></br>

## 인코딩 (`--encoding`)

기본값은 BOM 없는 **UTF-8** 입니다. 대부분의 경우 그대로 두면 됩니다. 일부 소비자가 UTF-16 이나 다른 인코딩을 요구한다면 지정합니다.

지원 인코딩:

|값|의미|
|-|-|
|`UTF-8`|BOM 없는 UTF-8 (기본)|
|`UTF-16`|UTF-16 LE|
|`UTF-32`|UTF-32|
|`ASCII`|ASCII|
|그 외|.NET `Encoding.GetEncoding(name)` 으로 처리. 예: `EUC-KR`, `Windows-1252`|

</br></br></br>

## 실행 예시

### 기본

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv
```

### 시작 셀 합의가 `B3` 인 프로젝트

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3
```

### 빌드 버전별로 산출물 분리

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --start-cell B3 ^
  --version 1.2.3-build.42
```

### 로그를 파일로 남기며 상세하게

```bash
ExcelColumnExtractor.exe ^
  --record-path ./Records ^
  --excel-path ./Excels ^
  --output-path ./Csv ^
  --log-path ./Logs ^
  --min-log-level Debug
```

</br></br></br>

## bat 으로 묶어 두기

추출기는 빌드 단계에서 자주 호출되므로 bat 으로 묶어 두면 편합니다.

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

추출 실패 시 종료 코드가 0 이 아니므로 `errorlevel` 로 분기할 수 있습니다.

같은 명령을 GitHub Actions 같은 CI 의 한 스텝으로 등록해 두면 빌드 과정에 그대로 통합됩니다. 추출 실패 시 종료 코드가 0 이 아니므로 그대로 워크플로우 실패로 이어져, 잘못된 데이터가 머지되기 전에 드러납니다.

</br></br></br>

## 권장 워크플로우

1. 한 프로젝트 안에서 `--start-cell` 위치를 하나로 합의합니다 (예: `B3` — `A` 열과 `1`, `2` 행은 시트 자유 영역).
2. 빌드 파이프라인에 추출기 호출을 한 단계로 둡니다.
3. 산출 CSV 는 런타임 빌드 출력 폴더로 복사되어 `StaticDataManager.LoadAsync` 가 읽습니다 ([3.5](../03-usage/05-static-data-manager.md) 참고).
4. 빌드 버전을 데이터에 기록해 두고 싶다면 `--version` 으로 출력 폴더를 분리합니다.

추출 자체에서 거르는 검증은 다음 네 가지입니다.

- **Record 스키마 결함** — 추출기가 Roslyn 으로 `.cs` 파일을 파싱해 잘못된 Attribute 사용, 지원하지 않는 타입 등을 잡습니다 (IDE 빌드 시점이 아니라 추출기 실행 시점에 동작).
- **헤더 누락** — Record 가 요구한 컬럼이 시트에 없을 때.
- **셀 값-타입 호환성** — `[Range]`, `[RegularExpression]`, `[DateTimeFormat]`, `[Length]`, `[CountRange]`, enum 멤버 등 셀 값이 Record 의 타입/Attribute 와 어긋날 때.
- **Primary Key 중복** — `[Key]` 가 붙은 컬럼의 시트 내 값 중복.

외래 키 (`[ForeignKey]`, `[SwitchForeignKey]`) 검증은 추출 단계가 아니라 런타임 (`LoadAsync`) 에서 일어납니다 ([3.6](../03-usage/06-foreign-keys.md) 참고).

---

[← 이전: 4.1 StaticDataHeaderGenerator](./01-header-generator.md) | [목차](../README.md) | [다음: 5.1 지원 타입 (Schemata) →](../05-advanced/01-schemata.md)
