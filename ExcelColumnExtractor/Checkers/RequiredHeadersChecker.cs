using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Parsers;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class RequiredHeadersChecker
{
    public sealed record TargetColumnIndices(IReadOnlyList<string> Headers, IReadOnlySet<int> IndexSet);

    public static TargetColumnIndicesContainer Check(
        IReadOnlyList<RecordSchema> staticDataRecordSchemaList,
        RecordSchemaContainer recordSchemaContainer,
        ExcelSheetNameContainer sheetNameContainer,
        ILogger logger)
    {
        var result = new Dictionary<RecordSchema, TargetColumnIndices>(staticDataRecordSchemaList.Count);

        var sb = new StringBuilder();
        foreach (var staticDataRecordSchema in staticDataRecordSchemaList)
        {
            try
            {
                var targetColumnIndexSet = CheckAndGetTargetColumns(
                    staticDataRecordSchema,
                    recordSchemaContainer,
                    sheetNameContainer,
                    logger);

                result.Add(staticDataRecordSchema, targetColumnIndexSet);
            }
            catch (Exception e)
            {
                var msg = $"{staticDataRecordSchema.RecordName.FullName}: {e.Message}";
                sb.AppendLine(msg);
                LogError(logger, staticDataRecordSchema, msg, e);
            }
        }

        if (sb.Length > 0)
        {
            throw new AggregateException(sb.ToString());
        }

        return new(result);
    }

    private static TargetColumnIndices CheckAndGetTargetColumns(
        RecordSchema staticDataRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        ExcelSheetNameContainer sheetNameContainer,
        ILogger logger)
    {
        var excelSheetName = sheetNameContainer.Get(staticDataRecordSchema);
        var sheetHeaders = SheetHeaderScanner.Scan(excelSheetName, logger);
        var standardHeaders = BuildStandardHeaders(staticDataRecordSchema, recordSchemaContainer, sheetHeaders, logger);

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

    private static IReadOnlyList<string> BuildStandardHeaders(
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyList<string> sheetHeaders,
        ILogger logger)
    {
        var lengthRequiredNames = recordSchema.DetectLengthRequiringFields(recordSchemaContainer);
        var containerLengths = HeaderLengthParser.Parse(sheetHeaders, lengthRequiredNames);
        return recordSchema.Flatten(recordSchemaContainer, containerLengths, logger);
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
