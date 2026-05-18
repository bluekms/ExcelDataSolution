# StaticDataPipeline

## 라이브러리
* StaticDataAttribute
* SchemaInfoScanner
* ExtractedDataLoader

## CLI 프로그램
1. ExcelColumnExtractor
2. ExtractedDataValidator

## ExcelColumnExtractor

### 개요

1. C# 코드에서 스키마를 파악
2. 엑셀 시트에서 C# 코드가 읽어야 할 컬럼을 추려냄
3. 해당 컬럼만 csv 등으로 출력

### 특징

* 기본적으로 C# 클래스 이름이 곧 엑셀 시트 이름이 된다
* 기본적으로 클래스 멤버 이름이 곧 엑셀 시트의 컬럼 이름이 된다
* 이름은 Attribute로 따로 지정할 수 있다
* Attribute로 타입을 지정하지 않으면 C#에 선언된 타입대로 셀을 읽는다

### 사용법

```
ExcelColumnExtractor <C#클래스경로> <엑셀파일경로> <출력파일경로>
```


## ExtractedDataValidator

### 개요

ExtractedDataLoader로 읽어온 데이터를 C# 코드 기준으로 검증하는 프로그램

### 사용법

```
ExtractedDataValidator <C#클래스경로> <출력파일경로>
```
