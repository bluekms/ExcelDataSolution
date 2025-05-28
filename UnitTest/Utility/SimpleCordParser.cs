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
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        return new(recordSchemaContainer, recordSchemaContainer.StaticDataRecordSchemata);
    }
}
