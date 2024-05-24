using System.Collections.Immutable;
using ExcelColumnExtractor.Checkers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using Xunit.Abstractions;

namespace UnitTest;

public class MaxCountAttributeTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public MaxCountAttributeTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    /*
    [Fact]
    public void NullablePrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] HashSet<int?> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void NullablePrimitiveHashSetFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] HashSet<int?> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2", "Point3"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void NullablePrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] List<int?> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void NullablePrimitiveListFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] List<int?> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2", "Point3"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void PrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] HashSet<int> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void PrimitiveHashSetFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] HashSet<int> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2", "Point3"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void PrimitiveKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student([Key] string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [MaxCount(2)] Dictionary<string, Student> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1.Name", "Point1.Age", "Point1.Score",
            "Point2.Name", "Point2.Age", "Point2.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void PrimitiveKeyRecordValueDictionaryFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student([Key] string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [MaxCount(2)] Dictionary<string, Student> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1.Name", "Point1.Age", "Point1.Score",
            "Point2.Name", "Point2.Age", "Point2.Score",
            "Point3.Name", "Point3.Age", "Point3.Score"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void PrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] List<int> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void PrimitiveListFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] List<int> Point
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point1", "Point2", "Point3"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void RecordHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] HashSet<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score",
            "Student2.Name", "Student2.Age", "Student2.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordHashSetInListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, [MaxCount(3)] List<int> Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] HashSet<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score1", "Student1.Score2", "Student1.Score3",
            "Student2.Name", "Student2.Age", "Student2.Score1", "Student2.Score2", "Student2.Score3"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordHashSetFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] HashSet<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score",
            "Student2.Name", "Student2.Age", "Student2.Score",
            "Student3.Name", "Student3.Age", "Student3.Score"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void RecordKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(int Id, string Name, int Age);

            public sealed record StudentScore([Key] Student Student, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [MaxCount(2)] Dictionary<Student, StudentScore> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1.Id", "Point1.Name", "Point1.Age", "Point1.Score",
            "Point2.Id", "Point2.Name", "Point2.Age", "Point2.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordKeyRecordValueInListDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(int Id, string Name, int Age);

            public sealed record StudentScore([Key] Student Student, List<int> Scores);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [MaxCount(2)] Dictionary<Student, StudentScore> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1.Id", "Point1.Name", "Point1.Age", "Point1.Score",
            "Point2.Id", "Point2.Name", "Point2.Age", "Point2.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordKeyRecordValueDictionaryFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(int Id, string Name);

            public sealed record StudentScore([Key] Student Student, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [MaxCount(2)] Dictionary<Student, StudentScore> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1.Id", "Point1.Name", "Point1.Score",
            "Point2.Id", "Point2.Name", "Point2.Score",
            "Point3.Id", "Point3.Name", "Point3.Score"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void RecordListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] List<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score",
            "Student2.Name", "Student2.Age", "Student2.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordListInListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, [MaxCount(3)] List<int> Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] HashSet<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score1", "Student1.Score2", "Student1.Score3",
            "Student2.Name", "Student2.Age", "Student2.Score1", "Student2.Score2", "Student2.Score3"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordListFailTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [MaxCount(2)] List<Student> Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Student1.Name", "Student1.Age", "Student1.Score",
            "Student2.Name", "Student2.Age", "Student2.Score",
            "Student3.Name", "Student3.Age", "Student3.Score"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }

    [Fact]
    public void RecordFailTest()
    {
        // TODO 예제 수정
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [MaxCount(1)] Student Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Child.Name", "Child.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }
    */
}
