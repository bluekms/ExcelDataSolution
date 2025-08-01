using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class FindLengthRequiredNamesTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MyClassFindTest()
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
        if (factory.CreateLogger<FindLengthRequiredNamesTest>() is not TestOutputLogger<FindLengthRequiredNamesTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);
        var results = LengthRequiringFieldDetector.Detect(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            logger);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectA.QuarterScore",
            "SubjectB",
            "SubjectB.QuarterScore",
        };

        Assert.Equal(expected, results);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithNameAttributesFindTest()
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
        if (factory.CreateLogger<FindLengthRequiredNamesTest>() is not TestOutputLogger<FindLengthRequiredNamesTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);
        var results = LengthRequiringFieldDetector.Detect(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            logger);

        var expected = new HashSet<string>
        {
            "SubjectF",
            "SubjectF.Scores",
            "SubjectB",
            "SubjectB.Scores",
        };

        Assert.Equal(expected, results);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithSingleColumnFindTest()
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
        if (factory.CreateLogger<FindLengthRequiredNamesTest>() is not TestOutputLogger<FindLengthRequiredNamesTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);
        var results = LengthRequiringFieldDetector.Detect(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            logger);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectB",
        };

        Assert.Equal(expected, results);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void MyClassWithSingleColumnAndColumnNameFindTest()
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
        if (factory.CreateLogger<FindLengthRequiredNamesTest>() is not TestOutputLogger<FindLengthRequiredNamesTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);
        var results = LengthRequiringFieldDetector.Detect(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            logger);

        var expected = new HashSet<string>
        {
            "SubjectA",
            "SubjectB",
        };

        Assert.Equal(expected, results);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void CompanyFindTest()
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
        if (factory.CreateLogger<FindLengthRequiredNamesTest>() is not TestOutputLogger<FindLengthRequiredNamesTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var parseResult = SimpleCordParser.Parse(code, logger);
        var results = LengthRequiringFieldDetector.Detect(
            parseResult.RawRecordSchemata[0],
            parseResult.RecordSchemaCatalog,
            logger);

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
        Assert.Empty(logger.Logs);
    }
}
