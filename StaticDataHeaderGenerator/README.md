# StaticDataHeaderGenerator
이 프로그램은 C# Static Date Record를 보고 평탄화(Flatten)하여 StaticData의 표준 Header를 생성합니다.
개발 복잡도를 낮추기 위해 이 프로그램은 하나의 Record만 처리합니다. <- 사용성이 너무 떨어짐


## 개요
* C#의 레코드를 읽어서 표준 Header 이름을 생성합니다.
* 레코드 맴버 중 포함 된 배열들을 위해 길이 정보가 필요합니다.
* 이 프로그램은 길이 정보 템플릿을 쉽게 생성하고, 이를 토대로 표준 Header 이름을 생성합니다.


## 실행 방법

### 길이 정보 파일 생성
`StaticDataHeaderGenerator.exe length <레코드 파일 경로> <길이 정보 파일 출력 경로> [<로그 파일 출력 경로>] [<최소 로그 레벨>]`

* 레코드 파일 경로: (필수) StaticDataRecord 들이 정의된 *.cs 파일들의 경로
* 길이 정보 파일 경로: (필수) 생성된 길이 정보 파일의 경로
* 로그 파일 출력 경로: (선택) 로그 파일 경로
* 최소 로그 레벨: (선택) 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)

### 헤더 생성
`StaticDataHeaderGenerator.exe header <레코드 파일 경로> <길이 정보 파일 경로> <출력 파일 경로> [<로그 파일 출력 경로>] [<최소 로그 레벨>]`

* 레코드 파일 경로: (필수) 대상이 되는 레코드 파일명 (*.cs)
* 길이 정보 파일 경로: (필수) 대상을 Header 로 변환할 때 필요한 배열 길이 정보 (배열이 없다고 해도 파일 자체는 필수)
* 출력 파일 경로: (필수) 생성된 Header 이름이 적혀 있는 파일 경로
* 로그 파일 출력 경로: (선택) 로그 파일 경로
* 최소 로그 레벨: (선택) 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)
