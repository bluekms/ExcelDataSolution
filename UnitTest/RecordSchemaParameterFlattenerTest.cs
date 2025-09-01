using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordSchemaParameterFlattenerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MyClassTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                ImmutableArray<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                ImmutableArray<Subject> SubjectA,
                int Age,
                ImmutableArray<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(29, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithSingleColumnTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                [SingleColumnCollection("", "")] ImmutableArray<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                ImmutableArray<Subject> SubjectA,
                int Age,
                ImmutableArray<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(16, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithSingleColumnAndColumnNameTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                [SingleColumnCollection("", "")][ColumnName(""QuarterScores"")] ImmutableArray<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                ImmutableArray<Subject> SubjectA,
                int Age,
                ImmutableArray<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(16, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithNameAttributesTest()
    {
        var code = @"
            public sealed record Subject(
                [ColumnName(""Bar"")] string Name,
                [ColumnName(""Scores"")] ImmutableArray<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""SubjectF"")] ImmutableArray<Subject> SubjectA,
                int Age,
                ImmutableArray<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectF", 3 },
            { "SubjectF.Scores", 4 },
            { "SubjectB", 4 },
            { "SubjectB.Scores", 2 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(29, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void DictionaryWithPrimitiveKeyTest()
    {
        var code = @"
            public sealed record MyRecord([Key] int Id, int Value);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyStaticData(
                string Name,
                FrozenDictionary<int, MyRecord> MyDictionary,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "MyDictionary", 3 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(7, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void DictionaryWithRecordKeyTest()
    {
        var code = @"
            public sealed record Human(string Name, int Age);
            public sealed record MyRecord([Key] Human Human, int Value);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyStaticData(
                FrozenDictionary<Human, MyRecord> MyDictionary,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "MyDictionary", 3 },
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(9, results.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void CompanyTest()
    {
        var code = @"
            public sealed record Address(string Street, string City);
            public sealed record ContactInfo(string PhoneNumber, string Email);
            public sealed record Project(string ProjectName, ImmutableArray<string> TeamMembers, double Budget);
            public sealed record Department(string DepartmentName, ImmutableArray<Project> Projects);

            public sealed record Employee(
                string FullName,
                int Age,
                Address HomeAddress,
                ContactInfo Contact,
                string Position,
                ImmutableArray<Department> Departments
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record Company(
                string CompanyName,
                Address HeadquartersAddress,
                FrozenSet<Employee> Employees,
                ImmutableArray<Department> CoreDepartments
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaParameterFlattenerTest>() is not TestOutputLogger<RecordSchemaParameterFlattenerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "Employees", 5 },
            { "CoreDepartments", 3 },
            { "Employees.Departments", 2 },
            { "Employees.Departments.Projects", 4 },
            { "Employees.Departments.Projects.TeamMembers", 6 },
            { "CoreDepartments.Projects", 3 },
            { "CoreDepartments.Projects.TeamMembers", 4 }
        };

        var results = RecordFlattener.Flatten(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            collectionLengths,
            logger);

        foreach (var header in results)
        {
            testOutputHelper.WriteLine(header);
        }

        Assert.Equal(425, results.Count);
        Assert.Empty(logger.Logs);
    }
}
