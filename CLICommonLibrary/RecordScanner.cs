using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;

namespace CLICommonLibrary;

public static class RecordScanner
{
    public static RecordSchemaCatalog Scan(string csPath, ILogger logger)
    {
        var loadResults = RecordSchemaLoader.Load(csPath, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResults);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var exceptionCount = RecordComplianceChecker.TryCheck(recordSchemaCatalog, logger);

        return exceptionCount > 0
            ? throw new InvalidOperationException($"There are {exceptionCount} exceptions.")
            : recordSchemaCatalog;
    }
}
