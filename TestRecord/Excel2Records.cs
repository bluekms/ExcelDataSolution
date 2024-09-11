namespace Excel2;

// Excel1의 SameNameSheet와 이름이 같지만, namespace가 다르기 때문에 충돌이 발생하지 않음
[StaticDataRecord("Excel2", "SameNameSheet")]
public sealed record SameNameSheet(string Name, float Score);

public sealed record SubjectData([Key][ColumnName("Subject")] string Name, int Score);

// TODO ColumnPrefix 동작하지 않음
[StaticDataRecord("Excel2", "DictionarySheet")]
public sealed record DictionarySheet(string Name, [ColumnPrefix("Score")] Dictionary<string, SubjectData> Subjects);
