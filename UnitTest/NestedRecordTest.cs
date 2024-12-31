using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class NestedRecordTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TempTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<NestedRecordTest>() is not TestOutputLogger<NestedRecordTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   namespace TestNamespace

                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Apple(string name)
                   {
                       public sealed record Foo(int a)
                       {
                           public sealed record Bar(int a, int b);
                       }
                   }
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        // 아랐습니다.
        // recordSchemaContainer.RecordSchemaDictionary[recordSchemaCollector.RecordNames.Single(x => x.Name == "Bar")].NamedTypeSymbol.ContainingType.Name
        // recordSchemaContainer.RecordSchemaDictionary[recordSchemaCollector.RecordNames.Single(x => x.Name == "Bar")].NamedTypeSymbol.ContainingType.ContainingType.Name

        // 이제 파라메터에서도 되는지 확인(안 될거니까 방법을 찾자)
        Assert.Empty(logger.Logs);
    }
}
