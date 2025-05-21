using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;

namespace UnitTest.Utility;

public static class SimpleCordParser
{
    public sealed record Result(RecordSchemaContainer RecordSchemaContainer, IReadOnlyList<RecordSchema> RawRecordSchemata);

    public static Result Parse(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        return new(recordSchemaContainer, recordSchemaContainer.StaticDataRecordSchemata);
    }
}
