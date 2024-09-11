using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class FindLengthRequiredNamesTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public FindLengthRequiredNamesTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void MyClassFindTest()
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
        var results = parseResult.RecordSchema.FindLengthRequiredNames(parseResult.RecordSchemaContainer);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectA.QuarterScore",
            "SubjectB",
            "SubjectB.QuarterScore",
        };

        Assert.Equal(expected, results);
    }

    [Fact]
    public void MyClassWithNameAttributesFindTest()
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
        var results = parseResult.RecordSchema.FindLengthRequiredNames(parseResult.RecordSchemaContainer);

        var expected = new HashSet<string>
        {
            "SubjectF",
            "SubjectF.Scores",
            "SubjectB",
            "SubjectB.Scores",
        };

        Assert.Equal(expected, results);
    }

    [Fact]
    public void MyClassWithSingleColumnFindTest()
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
        var results = parseResult.RecordSchema.FindLengthRequiredNames(parseResult.RecordSchemaContainer);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectB",
        };

        Assert.Equal(expected, results);
    }

    [Fact]
    public void MyClassWithSingleColumnAndColumnNameFindTest()
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
        var results = parseResult.RecordSchema.FindLengthRequiredNames(parseResult.RecordSchemaContainer);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectB",
        };

        Assert.Equal(expected, results);
    }

    [Fact]
    public void CompanyFindTest()
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
        var results = parseResult.RecordSchema.FindLengthRequiredNames(parseResult.RecordSchemaContainer);

        var expected = new HashSet<string>
        {
            "Employees",
            "CoreDepartments",
            "Employees.Departments",
            "Employees.Departments.Projects",
            "Employees.Departments.Projects.TeamMembers",
            "CoreDepartments.Projects",
            "CoreDepartments.Projects.TeamMembers",
        };

        Assert.Equal(expected, results);
    }
}
