using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;

namespace UnitTest.Utility;

public static class SimpleCordParser
{
    public sealed record Result(RecordSchemaCatalog RecordSchemaCatalog, IReadOnlyList<RecordSchema> RawRecordSchemata);

    public static Result Parse(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return new(recordSchemaCatalog, recordSchemaCatalog.StaticDataRecordSchemata);
    }
}
