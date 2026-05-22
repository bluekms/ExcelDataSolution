# 5.4 유효성 검사 개요

Sdp 의 유효성 검사가 파이프라인의 어느 시점에서 동작하는지 한 페이지로 정리합니다. 개별 Attribute 의 상세는 [5.2 Attribute 카탈로그](./02-attributes.md) 를, 데이터 흐름 전체는 [1. 소개](../01-introduction.md) 를 참고하세요.

## 세 가지 검사 시점

|시점|수행 주체|대상|
|-|-|-|
|**선언 검사**|`ExcelColumnExtractor` · `StaticDataHeaderGenerator`|Record/Attribute **선언** 자체의 결함|
|**추출**|`ExcelColumnExtractor`|Excel **셀 값** 이 스키마·Attribute 와 호환되는지|
|**로드**|`StaticDataManager.LoadAsync`|런타임 적재 시점의 구조·값·참조 검사|

선언 검사와 추출은 빌드 파이프라인(오프라인)에서, 로드는 애플리케이션 런타임에서 동작합니다. 선언 검사는 `.cs` 소스를 분석하므로 `ExcelColumnExtractor` 와 `StaticDataHeaderGenerator` 양쪽 실행 시 모두 수행되고, 런타임에는 소스가 없어 동작하지 않습니다.

</br></br></br>

## 검사 항목별 동작 시점

`O` 는 해당 시점에 검사함, `—` 는 검사하지 않음을 뜻합니다.

### A. 타입·구조 스키마

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|지원 타입 여부|O|—|—|미지원 타입 거부|
|컬렉션 자체 nullable 금지|O|—|—|`ImmutableArray<T>?`·`FrozenSet<T>?`·`FrozenDictionary<,>?`|
|Nullable Record 원소 금지|O|—|—|Record 배열/셋/Map Value 의 nullable|
|순환 참조 금지|O|—|—|Record 가 자기 자신을 직·간접 포함|
|Map Key non-nullable|O|—|—||
|Map Key↔Value `[Key]` 타입 일치|O|—|—||

### B. Attribute 정합성

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|`[Length]` 필수|O|—|—|다중 컬럼 컬렉션|
|`[NullString]` 필수·오사용|O|—|—|nullable 필수, non-nullable 금지|
|`[DateTimeFormat]` 필수·오사용|O|—|—|`DateTime` 필수, 비-DateTime 금지|
|`[TimeSpanFormat]` 필수·오사용|O|—|—|`TimeSpan` 필수, 비-TimeSpan 금지|
|`[RegularExpression]` 타입|O|—|—|`string` / `string?` 전용|
|`[Range]` 적용 가능 타입|O|—|—|`bool`·컬렉션·record 금지|
|`[CountRange]` 정합성|O|—|—|`[SingleColumnCollection]` 필요, `[Length]` 배타, minCount>0|
|`[SingleColumnCollection]` 정합성|O|—|—|Map 불가, 원소 primitive 만|
|`[Key]` 정합성|O|—|O|Record 당 최대 1개·non-nullable 은 선언 검사 / Map Value 의 `[Key]` 존재는 선언 검사·로드 양쪽|
|`[StaticDataRecord]` 존재|O|O|O|선언 검사 대상 식별 / 추출 대상 0개 시 종료 / 로드 시 필수|

### C. 셀 값

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|헤더 존재|—|O|O||
|셀 값-타입 호환성|—|O|O|로드는 변환(`Convert.ChangeType`) 실패로 검출|
|enum 멤버 이름 유효성|—|O|O|로드는 `Enum.IsDefined` — `[Key]` enum 은 생략|
|`DateTime`/`TimeSpan` 포맷 일치|—|O|O|양쪽 동일 format 으로 `ParseExact`|
|`[Range]` 값 범위|—|O|O||
|`[RegularExpression]` 패턴 일치|—|O|O||
|`[CountRange]` 분할 원소 개수|—|O|O|단일 컬럼 컬렉션|
|Primary Key 시트 내 중복|—|O|—|`[Key]` 컬럼. 로드는 자동 검사 없음 — `UniqueIndex` 가 opt-in 으로 보장|

### D. 외래 키

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|`[ForeignKey]`·`[SwitchForeignKey]` 동시 부착 금지|O|—|O|양쪽 검사|
|`[SwitchForeignKey]` 중복 조건 금지|O|—|O|양쪽 검사|
|FK 타겟 TableSet 존재|—|—|O|이름 오타는 로드 전 타입 검사로, `disabledTables` 로 빠진 테이블 참조는 로드 후 참조 검증으로 검출 (둘 다 `FkTargetNotFound`)|
|FK 타겟이 `[SingleColumnCollection]` 아님|—|—|O||
|FK 타겟 컬럼 존재|—|—|O||
|`[SwitchForeignKey]` 조건 컬럼 존재|—|—|O||
|`[SwitchForeignKey]` 조건값 분기 매칭|—|—|O||
|FK 참조 값 존재|—|—|O|실제 참조 무결성|

### E. 로드 구조

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|TableSet 단일 생성자|—|—|O||
|테이블 파라미터 타입|—|—|O|`StaticDataTable<,>` 여부|
|테이블 생성자(`ImmutableArray`) 존재|—|—|O||
|ViewSet 단일 생성자|—|—|O||
|View 파라미터 타입·non-nullable|—|—|O||
|View 생성자(`TableSet`) 존재|—|—|O||
|`LoadAsync` 동시 진입 금지|—|—|O||
|`UniqueIndex` 키 중복|—|—|O|테이블/뷰 생성 시, opt-in|

### F. 사용자 정의 검증

|검사 항목|선언 검사|추출|로드|비고|
|-|-|-|-|-|
|테이블 자기 검증|—|—|O|`StaticDataTable.Validate()` override, 테이블 인스턴스화 직후|
|매니저 교차 검증|—|—|O|`StaticDataManager.Validate(TTableSet)` override, 모든 FK 검증 후|
|뷰 자기 검증|—|—|O|`StaticDataView.Validate()` override, 뷰 빌드 직후|

</br></br></br>

## 추출을 거친 CSV 만 운영에 올린다

셀 값 검사(C) 중 `[Range]`·`[RegularExpression]`·`[CountRange]` 는 추출과 로드 양쪽에서 같은 규칙으로 동작합니다. 그러나 Primary Key 중복이나 선언 정합성 검사(A·B)는 추출 단계(선언 검사 포함)에서만 수행됩니다. 추출 단계를 거치지 않은 CSV 를 런타임에 직접 넣으면 이런 검사가 빠지므로, 정상적인 파이프라인은 항상 추출기를 거친 CSV 만 운영에 올립니다.

---

[← 이전: 5.3 타입 브랜딩 패턴](./03-type-branding.md) | [목차](../README.md) | [다음: 6. 라이선스 →](../06-license.md)
