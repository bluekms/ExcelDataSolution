using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class PropertyNameTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void NestedFullNameTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PropertyNameTest>() is not TestOutputLogger<PropertyNameTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   namespace TestNamespace;

                   public enum Grades
                   {
                       A,
                       B,
                       C,
                       D,
                       F,
                   }

                   public sealed record SubjectGrade(string Name, Grades Grade);

                   public sealed record Student(string Name, List<SubjectGrade> Grades);

                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record School(string Name, List<Student> ClassA, List<Student> ClassB);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];

        foreach (var parameter in rawSchema.RecordPropertySchemata)
        {
        }

        Assert.Empty(logger.Logs);
    }
}
