using ExcelColumnExtractor.Scanners;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Extensions;

public static class RecordSchemaPopulator
{
    public static void Populate(
        this RawRecordSchema rawRecordSchema,
        SheetBodyScanner.RowData rowData,
        IReadOnlySet<int> targetColumnIndexSet)
    {
        // 이건 밖에서 해오자
        var targetData = rowData.Data
            .Where((item, index) => targetColumnIndexSet.Contains(index))
            .ToList();

        foreach (var parameter in rawRecordSchema.RawParameterSchemaList)
        {
            // 탐색해서 targetData를 하나씩 집어넣으면서 타입을 확인한다.. 이건 집어넣는거네 체크 메서드를 분리해서 리턴해서 나오는거로 하자
            // 집어넣지는 말고 변환만 보자
            // 순회하는 코드 찾아오자. 타입체커쪽에 있을 듯
        }
    }
}
