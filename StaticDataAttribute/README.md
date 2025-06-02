# StaticDataAttribute

## Attributes
* ColumnName
* DateTime
* ForeignKey
* Ignore
* Key
* MaxCount
* NullString
* Range
* RegularExpression
* SingleColumnCollection
* StaticDataRecord
* TimeSpan


## Details

### ColumnNameAttribute
record 의 property 이름과 excel sheet 의 column 이름을 매핑

### DateTimeAttribute
```
https://learn.microsoft.com/ko-kr/dotnet/standard/base-types/standard-date-and-time-format-strings?form=MG0AV3
```
record 의 property 가 DateTime 형식일 때, 포멧을 지정 </br>
반드시 사용되어야 함

### ForeignKeyAttribute
외례키 검증
record struct Identifier 를 사용한다면 필요없거나 자동으로 해주거나 해야 함

### IgnoreAttribute
무시되는 컬럼

### KeyAttribute
```
Dictionary<int, int> // 불필요
Dictionary<int, Foo> // Foo의 int property 중 하나에 반드시 사용
Dictionary<Foo, Bar> // Bar의 Foo property 중 하나에 반드시 사용
```
record 의 property 가 Dictionary 형식일 때, 값은 record 라면 </br>
반드시 사용되어야 함

### MaxCountAttribute
record 의 property 가 Collection 형식일 때, 최대 개수를 지정

### NullStringAttribute
record 의 property 가 Nullable 형식일 때, null 값으로 처리할 문자열들을 지정

### RangeAttribute
record 의 property 범위를 지정

### RegularExpressionAttribute
record 의 property 가 정규식으로 검증되어야 할 때 사용

### SingleColumnCollectionAttribute
record 의 property 가 Collection 형식일 때, Excel sheet 의 단일 컬럼으로 매핑

### StaticDataRecordAttribute
Excel sheet 의 row와 매핑되는 record 형식에 사용

### TimeSpanAttribute
```
https://learn.microsoft.com/ko-kr/dotnet/standard/base-types/standard-timespan-format-strings
```
record 의 property 가 TimeSpan 형식일 때, 포멧을 지정 </br>
반드시 사용되어야 함
