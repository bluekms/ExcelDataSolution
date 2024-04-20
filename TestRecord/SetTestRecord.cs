namespace MyProject;

[StaticDataRecord]
public sealed record SetTestRecord(
    [Key]
    [Order]
    int Id,

    [Order]
    [Range(-50, 60)]
    [ColumnPrefix("Value")]
    HashSet<int> ValueSet,
);
