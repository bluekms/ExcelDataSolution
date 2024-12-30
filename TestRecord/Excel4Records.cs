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

public sealed record Student(string Name, List<SubjectGrade> Grades);

[StaticDataRecord("Excel4", "School")]
public sealed record School(string Name, List<Student> ClassA, List<Student> ClassB);
