using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Aggregator;
using ExcelColumnExtractor.Exceptions;
using ExcelColumnExtractor.Mappings;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using SchemaInfoScanner.TypeCheckers;

namespace ExcelColumnExtractor.Checkers;

public static class DataBodyChecker
{
    public static void Check(
        MetadataCatalogs metadataCatalogs,
        ExtractedTableMap extractedTableMap,
        ILogger<Program> logger)
    {
        var sb = new StringBuilder();

        foreach (var (recordSchema, table) in extractedTableMap.SortedTables)
        {
            try
            {
                CheckTable(metadataCatalogs, recordSchema, table);
            }
            catch (Exception e)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{recordSchema}: {e.Message}");
                LogError(logger, recordSchema, e.Message, e);
            }
        }

        if (sb.Length > 0)
        {
            throw new DataBodyCheckerException(sb.ToString());
        }
    }

    private static void CheckTable(
        MetadataCatalogs metadataCatalogs,
        RecordSchema recordSchema,
        BodyColumnAggregator.ExtractedTable table)
    {
        foreach (var row in table.Rows)
        {
            var context = CompatibilityContext.CreateNoCollect(metadataCatalogs, row.Data);

            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                var startPosition = context.Position;

                if (SetTypeChecker.IsSupportedSetType(propertySchema.NamedTypeSymbol))
                {
                    var setContext = CompatibilityContext.CreateCollectKey(
                        context.MetadataCatalogs,
                        context.Cells,
                        startPosition);

                    propertySchema.CheckCompatibility(setContext);
                    setContext.ValidateNoDuplicates();

                    var consumed = setContext.Position - startPosition;
                    context.Skip(consumed);
                    continue;
                }

                if (MapTypeChecker.IsSupportedMapType(propertySchema.NamedTypeSymbol))
                {
                    var mapContext = CompatibilityContext.CreateCollectKey(
                        context.MetadataCatalogs,
                        context.Cells,
                        startPosition);

                    propertySchema.CheckCompatibility(mapContext);
                    mapContext.ValidateNoDuplicates();

                    var consumed = mapContext.Position - startPosition;
                    context.Skip(consumed);
                    continue;
                }

                propertySchema.CheckCompatibility(context);
            }
        }
    }

    private static readonly Action<ILogger, RecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RecordSchema, string>(LogLevel.Error, new EventId(0, nameof(DataBodyChecker)), "{RecordSchema}: {ErrorMessage}");
}
