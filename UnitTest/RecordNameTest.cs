using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordNameTest(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("")]
    [InlineData("Namespace1.MyRecord.")]
    public void ThrowException(string input)
    {
        Assert.Throws<ArgumentException>(() => new RecordName(input));
    }

    [Fact]
    public void RawNestedFullNameTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordNameTest>() is not TestOutputLogger<RecordNameTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   namespace TestNamespace;

                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Level1(string name)
                   {
                       public sealed record Level2(int a)
                       {
                           public sealed record Level3(int a, int b);
                       }
                   }
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawSchema = recordSchemaCatalog.FindAll("Level2").Single();

        Assert.Empty(logger.Logs);
        Assert.Equal("TestNamespace.Level1.Level2", rawSchema.NestedFullName);
    }

    [Fact]
    public void NestedFullNameTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordNameTest>() is not TestOutputLogger<RecordNameTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   namespace TestNamespace;

                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Level1(string name)
                   {
                       public sealed record Level2(int a)
                       {
                           public sealed record Level3(int a, int b);
                       }
                   }
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawSchema = recordSchemaCatalog.FindAll("Level2").Single();
        var schema = RecordSchemaFactory.Create(
            rawSchema,
            recordSchemaCatalog,
            new Dictionary<string, int>());

        Assert.Empty(logger.Logs);
        Assert.Equal("TestNamespace.Level1.Level2", schema.NestedFullName);
    }
}
