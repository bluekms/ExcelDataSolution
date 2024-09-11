# StaticDataHeaderGenerator
이 프로그램은 C# Static Date Record를 토대로 데이터 시트를 작업할 때 사용하는 표준 헤더를 생성하는 프로그램이다.

## 표준 해더란

| Name | Score1 | Score2 | Score3 |
|------|--------|--------|--------|
| AAA  | 100    | 80     | 60     |

일반적인 Data Sheet를 작업할 때 컬럼 이름은 인간 친화적으로 작성됨

| Name | Score[0] | Score[1] | Score[2] |
|------|----------|----------|----------|
| AAA  | 100      | 80       | 60       |

표준 헤더는 프로그램이 계층구조를 파악할 수 있도록 디자인

|Name|Subjects[0].Subject|Subjects[0].Score|Subjects[1].Subject|Subjects[1].Score|Subjects[2].Subject|Subjects[2].Score|
|----|-------------------|-----------------|-------------------|-----------------|-------------------|-----------------|
|AAA |Math               |100              |Korean             |80               |English            |60               |

이를 이용해 복잡한 구조를 가진 데이터 시트를 더 적은 노력으로 C# 코드에서 쉽게 불러오는 것을 목표로 함

## 표준 헤더 생성 과정

``` DictionarySheet.cs
public sealed record SubjectData(
    [Key] string Subject,
    int Score);

[StaticDataRecord("Excel2", "DictionarySheet")]
public sealed record DictionarySheet(
    string Name,
    Dictionary<string, SubjectData> Subjects);
```

위와 같은 record.cs 파일이 있다고 할 때, [길이 파일을 생성](#길이-파일-생성)하면 다음과 같은 ini 파일이 생성됨

``` ExcelFileName.DictionarySheet.ini
[ExcelFileName.DictionarySheet]
Subjects =
```
사용자는 원하는 길이 값을 정수로 입력하고 저장. 여기서는 3으로 작성

``` StandardHeader
Name	Subjects[0].Subject	Subjects[0].Score	Subjects[1].Subject	Subjects[1].Score	Subjects[2].Subject	Subjects[2].Score
```
[생성된 헤더](#헤더-파일-생성)는 위와 같다.
이를 파일로 저장하거나 콘솔에서 바로 복사&붙여넣기를 이용해 데이터 시트를 작성할 수 있음

## 길이 파일 생성
### length 단일 레코드에 대한 길이 파일 생성
`StaticDataHeaderGenerator.exe length -r [레코드 파일 경로] -n [레코드 이름] -i [생성할 ini 파일 경로]`

#### 추가 옵션
* -l, --log-path : 로그 파일 경로
* -v : 최소 로그 레벨. 기본값 : Information (Verbose, Debug, Information, Warning, Error, Fatal)

### all-length 여러 레코드에 대한 길이 파일 생성
`StaticDataHeaderGenerator.exe all-length -r [레코드 파일 경로] -i [생성할 ini 파일 경로]`

#### 추가 옵션
* -l, --log-path : 로그 파일 경로
* -v : 최소 로그 레벨. 기본값 : Information (Verbose, Debug, Information, Warning, Error, Fatal)

## 헤더 파일 생성
### header 단일 레코드에 대한 헤더 파일 생성
`StaticDataHeaderGenerator.exe header -r [레코드 파일 경로] -n [레코드 이름] -i [길이 ini 파일 경로] -o [생성할 헤더 파일 경로]`

#### 추가 옵션
* -s, --separator : 헤더 구분자. 기본값 : \t
* -o, --output-path : 헤더 파일 경로. 제공되지 않으면 콘솔에만 출력
* -l, --log-path : 로그 파일 경로
* -v : 최소 로그 레벨. 기본값 : Information (Verbose, Debug, Information, Warning, Error, Fatal)


### all-header 여러 레코드에 대한 헤더 파일 생성
`StaticDataHeaderGenerator.exe all-header -r [레코드 파일 경로] -n [레코드 이름] -i [길이 ini 파일 경로] -o [생성할 헤더 파일 경로]`

#### 추가 옵션
* -s, --separator : 헤더 구분자. 기본값 : \t
* -o, --output-path : 헤더 파일 경로. 제공되지 않으면 콘솔에만 출력
* -l, --log-path : 로그 파일 경로
* -v : 최소 로그 레벨. 기본값 : Information (Verbose, Debug, Information, Warning, Error, Fatal)
