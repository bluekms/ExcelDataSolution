using System.Globalization;
using ExcelColumnExtractor.Resources;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using Sdp.Attributes;

namespace ExcelColumnExtractor.Extensions;

public static class StartCellResolver
{
    private const int StartCellAttributeParameterIndex = 2;

    public static string Resolve(RecordSchema recordSchema, string fallbackStartCell)
    {
        if (!recordSchema.TryGetAttributeValue<StaticDataRecordAttribute, string>(
                StartCellAttributeParameterIndex,
                out var attrStartCell))
        {
            return fallbackStartCell;
        }

        if (string.IsNullOrWhiteSpace(attrStartCell))
        {
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                Messages.Composite.EmptyStartCellInAttribute,
                recordSchema.RecordName.FullName));
        }

        return attrStartCell;
    }
}
