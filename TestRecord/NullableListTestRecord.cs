namespace MyProject;

[StaticDataRecord]
public record NullableListTestRecord(
    [Key]
    [Order]
    int Id,

    [Order]
    [Range(-50, 60)]
    [MaxCount(5)]
    List<int?> Value,

    [Order]
    string? Info
);
