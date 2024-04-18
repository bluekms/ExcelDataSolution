using StaticDataAttribute;

namespace MyProject;

public sealed record SubjectGradeInfo2([Key] string Subject, Grades Grade);

[StaticDataRecord]
public sealed record DictionaryTestRecord(
    [Key]
    [Order]
    [RegularExpression(@"^202200\d\d$")]
    string StudentId,
    [Order] string Name,
    [Order] Dictionary<string, SubjectGradeInfo2> SubjectGradeInfoList,
    [Order] string? Note
);
