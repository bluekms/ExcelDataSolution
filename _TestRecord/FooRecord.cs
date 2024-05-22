namespace MyProject;

public sealed record NameAndScore(string Name, int Score);

public sealed record NameAndScoreAndAge([Key] NameAncScore NameAndScore, int Age);

[StaticDataRecord]
public sealed record MyClass(
    [ColumnPrefix("NameAndScore")] HashSet<NameAndScore> NameAncScores,
    int ClassValue,
);
