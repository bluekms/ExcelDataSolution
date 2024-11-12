using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordSchemaFactoryTest(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("int", "-30")]
    public void Test(string type, string value)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        {{type}} Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        var first = recordSchema.RecordParameterSchemaList[0];
        first.CheckCompatibility(value);
    }
}
