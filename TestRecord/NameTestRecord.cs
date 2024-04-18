using StaticDataAttribute;

namespace MyProject;

[StaticDataRecord]
[SheetName("이름테이블")]
public sealed record NameTestRecord(
    [Key]
    [Order]
    int Id,

    [ColumnName("값2")]
    [Order]
    int Value2,

    [Order]
    [Range(0, 20)]
    int Value3
);
