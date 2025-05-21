using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.HeaderProcessors;

public static class HeaderLengthBuilder
{
    public static HeaderLengthContainer Build(
        IReadOnlyList<RecordSchema> staticDataRecordSchemaList,
        RecordSchemaContainer recordSchemaContainer,
        ExcelSheetNameContainer sheetNameContainer,
        ILogger logger)
    {
        var headerLengths = new Dictionary<RecordSchema, IReadOnlyDictionary<string, int>>();
        foreach (var recordSchema in staticDataRecordSchemaList)
        {
            try
            {
                var excelSheetName = sheetNameContainer.Get(recordSchema);
                var sheetHeaders = SheetHeaderScanner.Scan(excelSheetName, logger);
                var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
                    recordSchema,
                    recordSchemaContainer,
                    logger);

                var containerLengths = HeaderLengthParser.Parse(sheetHeaders, lengthRequiredNames);

                headerLengths.Add(recordSchema, containerLengths);
            }
            catch (Exception e)
            {
                var msg = $"{recordSchema.RecordName.FullName}: {e.Message}";
                LogError(logger, recordSchema, msg, e);
            }
        }

        return new(headerLengths.AsReadOnly());
    }

    private static readonly Action<ILogger, RecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RecordSchema, string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{RecordSchema}: {ErrorMessage}");
}
