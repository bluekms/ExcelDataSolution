using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;

namespace CLICommonLibrary;

public static class RecordScanner
{
    public static RecordSchemaContainer Scan(string csPath, ILogger logger)
    {
        var loadResults = RecordSchemaLoader.Load(csPath, logger);
        var recordSchemaCollector = new RecordSchemaCollector();
        var enumMemberCollector = new EnumMemberCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
            enumMemberCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var exceptionCount = RecordComplianceChecker.TryCheck(recordSchemaContainer, logger);
        if (exceptionCount > 0)
        {
            throw new InvalidOperationException($"There are {exceptionCount} exceptions.");
        }

        return recordSchemaContainer;
    }
}
