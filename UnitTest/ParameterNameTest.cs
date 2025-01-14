using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class ParameterNameTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void NestedFullNameTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ParameterNameTest>() is not TestOutputLogger<ParameterNameTest> logger)
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawSchema = recordSchemaContainer.StaticDataRecordSchemata[0];

        foreach (var parameter in rawSchema.RawParameterSchemaList)
        {
        }

        Assert.Empty(logger.Logs);
    }
}
