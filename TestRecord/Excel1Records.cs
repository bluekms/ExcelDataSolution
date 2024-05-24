namespace Excel1;

[StaticDataRecord("Excel1", "FirstSheet")]
public sealed record FirstSheet(string Name, int Score);

[StaticDataRecord("Excel1", "ArraySheet")]
public sealed record ArraySheet(string Name, [ColumnPrefixAttribute("Score")] List<int> Score);

[StaticDataRecord("Excel1", "SameNameSheet")]
public sealed record SameNameSheet(string Name, int Score);

[StaticDataRecord("Excel1", "SingleColumnContainerSheet")]
public sealed record SingleColumnContainerSheet(int Id, [SingleColumnContainer(", ")] List<float> Values);

