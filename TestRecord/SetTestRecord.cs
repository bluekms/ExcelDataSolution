namespace MyProject;

[StaticDataRecord]
public sealed rcord SetTestRecord(
    [Key]
    [Order]
    int Id,

    [Order]
    [Range(-50, 60)]
    [ColumnPrefix("Value")]
    HashSet<int> ValueSet,
);
