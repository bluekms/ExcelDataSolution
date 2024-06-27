using System.Collections.Immutable;
using ExcelColumnExtractor.NameObjects;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class HeaderChecker
{
    public static void Check(ImmutableList<RecordParameterSchema> parameterSchemaList, RecordSchemaContainer recordSchemaContainer, ImmutableList<string> headerList, SheetName sheetName)
    {
    }
}
