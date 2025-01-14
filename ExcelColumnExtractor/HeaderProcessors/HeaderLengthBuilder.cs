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
        IReadOnlyList<RawRecordSchema> staticDataRecordSchemaList,
        RecordSchemaContainer recordSchemaContainer,
        ExcelSheetNameContainer sheetNameContainer,
        ILogger logger)
    {
        var headerLengths = new Dictionary<RawRecordSchema, IReadOnlyDictionary<string, int>>();
        foreach (var rawRecordSchema in staticDataRecordSchemaList)
        {
            try
            {
                var excelSheetName = sheetNameContainer.Get(rawRecordSchema);
                var sheetHeaders = SheetHeaderScanner.Scan(excelSheetName, logger);
                var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
                    rawRecordSchema,
                    recordSchemaContainer,
                    logger);

                var containerLengths = HeaderLengthParser.Parse(sheetHeaders, lengthRequiredNames);

                headerLengths.Add(rawRecordSchema, containerLengths);
            }
            catch (Exception e)
            {
                var msg = $"{rawRecordSchema.RecordName.FullName}: {e.Message}";
                LogError(logger, rawRecordSchema, msg, e);
            }
        }

        return new(headerLengths.AsReadOnly());
    }

    private static readonly Action<ILogger, RawRecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RawRecordSchema, string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{RecordSchema}: {ErrorMessage}");
}
