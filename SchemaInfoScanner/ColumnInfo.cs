using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner;

// 이게 액셀의 한 column이 된다
// 내부에 리커전이 있다면 이게 더 늘어나야 한다
public sealed record ColumnInfo(
    ITypeSymbol TypeSymbol,
    string RecordName,
    string? DataName,
    int Depth = 0)
{
    public override string ToString()
    {
        // 출력할 때 Depth만큼 뒤로 밀어주자
        if (DataName is null)
        {
            return $"{TypeSymbol}\t{RecordName}";
        }
        else
        {
            return $"{TypeSymbol}\t{RecordName}({DataName})";
        }
    }
}
