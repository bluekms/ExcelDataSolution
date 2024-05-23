namespace Excel1;

[StaticData]
public sealed record FirstSheet(string Name, int Score);

[StaticData]
public sealed record ArraySheet(string Name, [ColumnPrefixAttribute("Score")] List<int> Score);

[StaticData]
[SheetName("Excel1.SameNameSheet")]
public sealed record SameNameSheet(string Name, int Score);
