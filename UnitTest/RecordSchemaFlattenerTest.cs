using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordSchemaFlattenerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public RecordSchemaFlattenerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void MyClassTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(29, results.Count);
    }

    [Fact]
    public void MyClassWithSingleColumnTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                [SingleColumnContainer("", "")] List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(16, results.Count);
    }

    [Fact]
    public void MyClassWithSingleColumnAndColumnNameTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                [SingleColumnContainer("", "")][ColumnName(""QuarterScores"")] List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(16, results.Count);
    }

    [Fact]
    public void MyClassWithNameAttributesTest()
    {
        var code = @"
            public sealed record Subject(
                [ColumnName(""Bar"")] string Name,
                [ColumnName(""Scores"")] List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""SubjectF"")] List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectF", 3 },
            { "SubjectF.Scores", 4 },
            { "SubjectB", 4 },
            { "SubjectB.Scores", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(29, results.Count);
    }

    [Fact]
    public void DictionaryWithPrimitiveKeyTest()
    {
        var code = @"
            public sealed record MyRecord([Key] int Id, int Value);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyStaticData(
                string Name,
                Dictionary<int, MyRecord> MyDictionary,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "MyDictionary", 3 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(7, results.Count);
    }

    [Fact]
    public void DictionaryWithRecordKeyTest()
    {
        var code = @"
            public sealed record Human(string Name, int Age);
            public sealed record MyRecord([Key] Human Human, int Value);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyStaticData(
                Dictionary<Human, MyRecord> MyDictionary,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "MyDictionary", 3 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(9, results.Count);
    }

    [Fact]
    public void CompanyTest()
    {
        var code = @"
            public sealed record Address(string Street, string City);
            public sealed record ContactInfo(string PhoneNumber, string Email);
            public sealed record Project(string ProjectName, List<string> TeamMembers, double Budget);
            public sealed record Department(string DepartmentName, List<Project> Projects);

            public sealed record Employee(
                string FullName,
                int Age,
                Address HomeAddress,
                ContactInfo Contact,
                string Position,
                List<Department> Departments
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record Company(
                string CompanyName,
                Address HeadquartersAddress,
                HashSet<Employee> Employees,
                List<Department> CoreDepartments
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

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

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        foreach (var header in results)
        {
            this.testOutputHelper.WriteLine(header);
        }

        Assert.Equal(425, results.Count);
    }
}
