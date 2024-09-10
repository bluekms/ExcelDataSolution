namespace Excel3;

public enum Gender
{
    Male,
    Femail,
    Other,
}

public enum GradeLevel
{
    Freshman,
    Sophomore,
    Junior,
    Senior,
}

[StaticDataRecord("Excel3", "School")]
public record School(
    string? SchoolName,
    List<Student> Students,
    HashSet<string> AvailableCourses,
    Dictionary<int, Teacher> Teachers,
    Dictionary<Student, Enrollment> StudentEnrollments,
)
{
    // TODO 추후에 INamespaceSymbol 을 가져와서 namespace가 비어있다면 넣어주도록 수정
	// Identify 지원을 위해 필수
    //public record Enrollment([Key] Student Student, string? Course, GradeLevel? GradeLevel);
}

public record Enrollment([Key] Student Student, string? Course, GradeLevel? GradeLevel);

public record Student(
    int? StudentId,
    string? FirstName,
    string? LastName,
    Gender? Gender,
    List<float?> Grades,
    HashSet<string?> Extracurriculars);

public record Teacher([Key] int TeacherId, string? Name, string? Subject);
