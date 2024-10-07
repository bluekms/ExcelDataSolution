using System.Diagnostics.CodeAnalysis;
using ExcelColumnExtractor.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.RecordSchemaExtensions;
using StaticDataAttribute;

namespace ExcelColumnExtractor.Containers;

public sealed class ExcelSheetNameContainer(
    IReadOnlyDictionary<string, ExcelSheetName> sheetNames)
{
    public int Count => sheetNames.Count;

    public ExcelSheetName Get(RecordSchema recordSchema)
    {
        var values = recordSchema.GetAttributeValueList<StaticDataRecordAttribute>();
        var excelSheetNameString = $"{values[0]}.{values[1]}";

        return sheetNames[excelSheetNameString];
    }

    internal bool TryGet(string excelFileName, string sheetName, [MaybeNullWhen(false)] out ExcelSheetName excelSheetName)
    {
        var excelSheetNameString = $"{excelFileName}.{sheetName}";

        return sheetNames.TryGetValue(excelSheetNameString, out excelSheetName);
    }
}
