using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.HeaderProcessors;
using ExcelColumnExtractor.NameObjects;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class RequiredHeadersChecker
{
    public sealed record TargetColumnIndices(IReadOnlyList<string> Headers, IReadOnlySet<int> IndexSet);

    public static TargetColumnIndicesContainer Check(
        IReadOnlyList<RecordSchema> staticDataRecordSchemaList,
        RecordSchemaCatalog recordSchemaCatalog,
        ExcelSheetNameContainer sheetNameContainer,
        HeaderLengthContainer headerLengthContainer,
        ILogger logger)
    {
        var result = new Dictionary<RecordSchema, TargetColumnIndices>(staticDataRecordSchemaList.Count);

        var sb = new StringBuilder();
        foreach (var recordSchema in staticDataRecordSchemaList)
        {
            try
            {
                var excelSheetName = sheetNameContainer.Get(recordSchema);
                var headerLengths = headerLengthContainer.Get(recordSchema);

                var targetColumnIndexSet = CheckAndGetTargetColumns(
                    recordSchema,
                    recordSchemaCatalog,
                    excelSheetName,
                    headerLengths,
                    logger);

                result.Add(recordSchema, targetColumnIndexSet);
            }
            catch (Exception e)
            {
                var msg = $"{recordSchema.RecordName.FullName}: {e.Message}";
                sb.AppendLine(msg);
                LogError(logger, recordSchema, msg, e);
            }
        }

        return sb.Length > 0
            ? throw new AggregateException(sb.ToString())
            : new(result);
    }

    private static TargetColumnIndices CheckAndGetTargetColumns(
        RecordSchema recordSchema,
        RecordSchemaCatalog recordSchemaCatalog,
        ExcelSheetName excelSheetName,
        IReadOnlyDictionary<string, int> headerLengths,
        ILogger logger)
    {
        var sheetHeaders = SheetHeaderScanner.Scan(excelSheetName, logger);
        var standardHeaders = RecordFlattener.Flatten(
            recordSchema,
            recordSchemaCatalog,
            headerLengths,
            logger);

        var targetColumnIndexSet = CheckAndGetTargetHeaderIndexSet(standardHeaders, sheetHeaders);
        var targetHeaders = targetColumnIndexSet.Select(index => sheetHeaders[index]).ToList();
        if (targetHeaders.Count != targetColumnIndexSet.Count)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Header and index count mismatch.");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Headers: {{ {string.Join(", ", targetHeaders)} }}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"IndexSet: {{ {string.Join(", ", targetColumnIndexSet)} }}");

            throw new ArgumentException(sb.ToString());
        }

        return new(targetHeaders, targetColumnIndexSet);
    }

    private static HashSet<int> CheckAndGetTargetHeaderIndexSet(IReadOnlyList<string> standardHeaders, IReadOnlyList<string> sheetHeaders)
    {
        var targetHeaderIndexSet = new HashSet<int>();
        foreach (var standardHeader in standardHeaders)
        {
            var index = CaseInsensitiveIndexOf(sheetHeaders, standardHeader);
            if (index is -1)
            {
                var sb = new StringBuilder();
                sb.AppendLine(CultureInfo.InvariantCulture, $"Header not found: {standardHeader}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"SheetHeaders: {{ {string.Join(", ", sheetHeaders)} }}");
                sb.AppendLine(CultureInfo.InvariantCulture, $"RecordHeaders: {{ {string.Join(", ", standardHeaders)} }}");

                throw new ArgumentException(sb.ToString());
            }

            if (!sheetHeaders[index].Equals(standardHeader, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Header case sensitivity: {standardHeader}");
            }

            targetHeaderIndexSet.Add(index);
        }

        return targetHeaderIndexSet;
    }

    private static int CaseInsensitiveIndexOf(IReadOnlyList<string> list, string value)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static readonly Action<ILogger, RecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RecordSchema, string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{RecordSchema}: {ErrorMessage}");
}
