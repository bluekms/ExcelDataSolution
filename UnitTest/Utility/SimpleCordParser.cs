using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace UnitTest.Utility;

public static class SimpleCordParser
{
    public sealed record Result(RawRecordSchema RawRecordSchema, RecordSchemaContainer RecordSchemaContainer);

    public static Result Parse(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        return new(recordSchema, recordSchemaContainer);
    }

    // TODO 이거 리턴형식이 같은 컨테이너를 리스트로 반환하고 있다
    public static List<Result> ParseAll(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        return recordSchemaContainer.StaticDataRecordSchemata
            .Select(x => new Result(x, recordSchemaContainer))
            .ToList();
    }
}
