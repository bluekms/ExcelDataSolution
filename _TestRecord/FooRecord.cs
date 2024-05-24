﻿namespace MyProject;

public sealed record NameAndScore(string Name, int Score);

public sealed record NameAndScoreAndAge([Key] NameAndScore NameAndScore, int Age);

[StaticDataRecord]
public sealed record MyClass(
    [ColumnPrefix("NameAndScore")] Dictionary<NameAndScore, NameAndScoreAndAge> NameAndScores,
    int ClassValue,
);
