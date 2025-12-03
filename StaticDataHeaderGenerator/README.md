# StaticDataHeaderGenerator
이 프로그램은 C# Static Data Record를 토대로 데이터 시트를 작업할 때 사용하는 표준 헤더를 생성하는 프로그램입니다.

## 표준 해더 정의

| Name | Score1 | Score2 | Score3 |
|------|--------|--------|--------|
| AAA  | 100    | 80     | 60     |

일반적으로 엑셀 시트는 위와 같이 작업됩니다.

| Name | Score[0] | Score[1] | Score[2] |
|------|----------|----------|----------|
| AAA  | 100      | 80       | 60       |

표준 해더란 Static Data Solution 에서 읽기 쉬운 위와 같이 작업된 헤더를 뜻합니다.

|Name|Subjects[0].Subject|Subjects[0].Score|Subjects[1].Subject|Subjects[1].Score|Subjects[2].Subject|Subjects[2].Score|
|----|-------------------|-----------------|-------------------|-----------------|-------------------|-----------------|
|AAA |Math               |100              |Korean             |80               |English            |60               |

이 툴은 복잡한 계층구조를 가진 record 파일을 표준 헤더로 출력해 줍니다.

## 표준 헤더 생성 과정

``` DictionarySheet.cs
public sealed record SubjectData(
    [Key] string Subject,
    int Score);

[StaticDataRecord("Excel2", "DictionarySheet")]
public sealed record DictionarySheet(
    string Name,

    [ColumnName("SubjectAndScore")][Length(3)]
    FrozenDictionary<string, SubjectData> Subjects);
```

프로그래머가 위와 같은 record.cs를 작업했다면

``` StandardHeader
  - Name
  - Subjects[0].Subject
  - Subjects[0].Score
  - Subjects[1].Subject
  - Subjects[1].Score
  - Subjects[2].Subject
  - Subjects[2].Score
```
이 프로그램을 이용해 이와 같은 표준 헤더를 얻을 수 있습니다.

## 사용 방법
`StaticDataHeaderGenerator.exe -r [레코드 파일 경로] -o [생성할 헤더 파일 경로]`
`StaticDataHeaderGenerator.exe -r [레코드 파일 경로] -n [레코드명] -o [생성할 헤더 파일 경로]`

### 옵션 설명
* -r, --record-path : 레코드 파일 경로
* -n, --record-name : 레코드명 (없다면 경로명의 모든 레코드를 대상으로 출력)
* -s, --separator : 헤더 구분자. 기본값 : \t
* -o, --output-file : 헤더 파일 경로. 제공되지 않으면 콘솔에만 출력
* -l, --log-path : 로그 파일 경로
* -v : 최소 로그 레벨. 기본값 : Information (Verbose, Debug, Information, Warning, Error, Fatal)
