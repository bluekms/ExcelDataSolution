using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;

namespace CLICommonLibrary;

public static class RecordScanner
{
    public static MetadataCatalogs Scan(string csPath, ILogger logger)
    {
        var loadResults = RecordSchemaLoader.Load(csPath, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResults, logger);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var exceptionCount = RecordComplianceChecker.TryCheck(recordSchemaCatalog, logger);
        if (exceptionCount > 0)
        {
            logger.LogError("There are {ExceptionCount} exceptions.", exceptionCount);
        }

        var enumMemberCatalog = new EnumMemberCatalog(loadResults);
        return new(recordSchemaCatalog, enumMemberCatalog);
    }

    public static async Task<MetadataCatalogs> ScanAsync(string csPath, ILogger logger, CancellationToken cancellationToken = default)
    {
        var loadResults = await RecordSchemaLoader.LoadAsync(csPath, logger, cancellationToken);
        var recordSchemaSet = new RecordSchemaSet(loadResults, logger);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var exceptionCount = RecordComplianceChecker.TryCheck(recordSchemaCatalog, logger);
        if (exceptionCount > 0)
        {
            logger.LogError("There are {ExceptionCount} exceptions.", exceptionCount);
        }

        var enumMemberCatalog = new EnumMemberCatalog(loadResults);
        return new(recordSchemaCatalog, enumMemberCatalog);
    }
}
