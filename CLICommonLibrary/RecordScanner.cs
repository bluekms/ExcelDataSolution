using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;

namespace CLICommonLibrary;

public static class RecordScanner
{
    public static RecordSchemaContainer Scan(string csPath, ILogger logger)
    {
        var loadResults = Loader.Load(csPath, logger);
        var recordSchemaCollector = new RecordSchemaCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        Checker.Check(recordSchemaContainer, logger);
        var exceptionCount = Checker.TryCheck(recordSchemaContainer, logger);
        if (exceptionCount > 0)
        {
            throw new InvalidOperationException($"There are {exceptionCount} exceptions.");
        }

        return recordSchemaContainer;
    }
}
