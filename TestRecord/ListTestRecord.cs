using StaticDataAttribute;

namespace MyProject;

[StaticDataRecord]
public sealed record ListTestRecord(
    [Key]
    [Order]
    int Id,

    [Order]
    [Range(-50, 60)]
    [MaxCount(5)]
    List<int> ValueList,

    [Order]
    string? Info
);
