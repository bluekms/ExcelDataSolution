namespace Excel1;

[StaticDataRecord("Excel1", "FirstSheet")]
public sealed record FirstSheet(string Name, int Score);

[StaticDataRecord("Excel1", "ArraySheet")]
public sealed record ArraySheet1(string Name, [ColumnName("Score[1]")]int Score1, [ColumnName("Score[2]")] int Score3);

[StaticDataRecord("Excel1", "ArraySheet")]
public sealed record ArraySheet2(string Name, [ColumnName("Score")] List<int> Scores);

// Excel2의 SameNameSheet와 이름이 같지만, namespace가 다르기 때문에 충돌이 발생하지 않음
[StaticDataRecord("Excel1", "SameNameSheet")]
public sealed record SameNameSheet(string Name, int Score);

[StaticDataRecord("Excel1", "SingleColumnContainerSheet")]
public sealed record SingleColumnContainerSheet(int Id, [SingleColumnContainer(", ")] List<float> Values);

