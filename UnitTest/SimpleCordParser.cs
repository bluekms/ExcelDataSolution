using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace UnitTest;

public static class SimpleCordParser
{
    public sealed record Result(RecordSchema RecordSchema, RecordSchemaContainer RecordSchemaContainer);

    public static Result Parse(string code, ILogger logger)
    {
        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values
            .Single(x => x.HasAttribute<StaticDataRecordAttribute>());

        return new(recordSchema, recordSchemaContainer);
    }
}
