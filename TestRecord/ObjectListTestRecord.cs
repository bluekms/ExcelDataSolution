using StaticDataAttribute;

namespace MyProject;

public sealed record SubjectGradeInfo(string Subject, Grades Grade);

[StaticDataRecord]
public sealed record ObjectListTestRecord(
    [Key]
    [Order]
    [RegularExpression(@"^202200\d\d$")]
    string StudentId,
    [Order] string Name,
    [Order] List<SubjectGradeInfo> SubjectGradeInfoList,
    [Order] string? Note
);
