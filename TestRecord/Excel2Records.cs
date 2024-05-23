namespace Excel2;

[StaticData]
[SheetName("Excel2.SameNameSheet")]
public sealed record SameNameSheet(string Name, int Score);

[StaticData]
public sealed record DictionarySheet(string Name, [ColumnPrefixAttribute("Score")] Dictionary<string, int> Score);
