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

[StaticDataRecord("Excel3", "SchoolSheet")]
public record School(
    [NullString("")] string? SchoolName,
    ImmutableArray<Student> Students,
    FrozenSet<string> AvailableCourses,
    FrozenDictionary<int, Teacher> Teachers,
    FrozenDictionary<Student, Enrollment> StudentEnrollments);

public record Enrollment(
    [Key]
    Student Student,
    [NullString("미수강")]
    string? Course,
    [NullString("")]
    GradeLevel? GradeLevel);

public record Student(
    [NullString("")] int? StudentId,
    [NullString("")] string? FirstName,
    [NullString("")] string? LastName,
    [NullString("")] Gender? Gender,
    [NullString("")] ImmutableArray<float?> Grades,
    [NullString("")] FrozenSet<string?> Extracurriculars);

public record Teacher(
    [Key]
    int TeacherId,
    [NullString("-")]
    string? Name,
    [NullString("미정")]
    string? Subject);
