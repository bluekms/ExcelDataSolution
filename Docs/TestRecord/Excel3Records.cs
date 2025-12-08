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
    [Length(3)] ImmutableArray<Student> Students,
    [Length(5)] FrozenSet<string> AvailableCourses,
    [Length(3)] FrozenDictionary<int, Teacher> Teachers,
    [Length(3)] FrozenDictionary<Student, Enrollment> StudentEnrollments);

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
    [NullString("")][Length(4)] ImmutableArray<float?> Grades,
    [NullString("")][Length(3)] FrozenSet<string?> Extracurriculars);

public record Teacher(
    [Key]
    int TeacherId,
    [NullString("-")]
    string? Name,
    [NullString("미정")]
    string? Subject);
