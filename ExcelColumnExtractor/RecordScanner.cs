using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;

namespace ExcelColumnExtractor;

public static class RecordScanner
{
    public static RecordSchemaContainer Scan(ILogger logger, string csPath)
    {
        var loadResults = Loader.Load(csPath);
        var recordSchemaCollector = new RecordSchemaCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var exceptionCount = Checker.TryCheck(recordSchemaContainer, logger);
        if (exceptionCount > 0)
        {
            throw new InvalidOperationException($"There are {exceptionCount} exceptions.");
        }

        return recordSchemaContainer;
    }
}
