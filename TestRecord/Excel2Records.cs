namespace Excel2;

[StaticDataRecord("Excel2", "SameNameSheet")]
public sealed record SameNameSheet(string Name, int Score);

public sealed record SubjectData([Key][ColumnName("Subject")] string Name, int Score);

[StaticDataRecord("Excel2", "DictionarySheet")]
public sealed record DictionarySheet(string Name, [ColumnPrefix("Score")] Dictionary<string, SubjectData> Subjects);
