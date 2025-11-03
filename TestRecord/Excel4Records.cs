namespace Excel4;

public enum Grades
{
    A,
    B,
    C,
    D,
    F,
}

// 과목은 "국", "영", "수" 3개로 고정
public sealed record SubjectGrade(string Name, Grades Grade);

public sealed record Student(
    string Name,
    [Length(3)] ImmutableArray<SubjectGrade> Grades);

[StaticDataRecord("Excel4", "School")]
public sealed record School(
    string Name, 
    [Length(3)] ImmutableArray<Student> ClassA,
    [Length(2)] ImmutableArray<Student> ClassB);
