using StaticDataAttribute;

namespace MyProject;

[StaticDataRecord]
public sealed record OrderTestRecord(
    [Key]
    [Order]
    int Id,

    [Order]
    [Range(0, 20)]
    int Value3,

    [Order]
    int Value1
);
