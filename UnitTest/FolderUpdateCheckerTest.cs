using ExcelColumnExtractor.Checkers;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class FolderUpdateCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var now = DateTime.UtcNow;
        var dummy = new Dictionary<string, DateTime>();
        dummy.Add("dummy", now);

        var before = new FolderState("DummyPath", dummy);
        var after = new FolderState("DummyPath", dummy);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;

        FolderUpdateChecker.Check(before, after, logger);
    }

    [Fact]
    public void ThrowDifferencePath()
    {
        var now = DateTime.UtcNow;
        var dummy = new Dictionary<string, DateTime>();
        dummy.Add("dummy", now);

        var before = new FolderState("PathA", dummy);
        var after = new FolderState("PathB", dummy);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;

        Assert.Throws<ArgumentException>(() => FolderUpdateChecker.Check(before, after, logger));
    }

    [Fact]
    public void AddedCheckTest()
    {
        var now = DateTime.UtcNow;
        var dummy = new Dictionary<string, DateTime>();
        dummy.Add("dummy", now);

        var added = new Dictionary<string, DateTime>();
        added.Add("dummy", now);
        added.Add("Foo", now);

        var before = new FolderState("DummyPath", dummy);
        var after = new FolderState("DummyPath", added);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;
        FolderUpdateChecker.Check(before, after, logger);

        Assert.Single(logger.Logs);
        Assert.Equal("File Foo was added.", logger.Logs.First().Message);
    }

    [Fact]
    public void RemoveCheckTest()
    {
        var now = DateTime.UtcNow;
        var dummy = new Dictionary<string, DateTime>();
        dummy.Add("dummy", now);
        dummy.Add("Foo", now);

        var removed = new Dictionary<string, DateTime>();
        removed.Add("dummy", now);

        var before = new FolderState("DummyPath", dummy);
        var after = new FolderState("DummyPath", removed);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;
        FolderUpdateChecker.Check(before, after, logger);

        Assert.Single(logger.Logs);
        Assert.Equal("File Foo was removed.", logger.Logs.First().Message);
    }

    [Fact]
    public void UpdateCheckTest()
    {
        var now = DateTime.UtcNow;
        var beforeState = new Dictionary<string, DateTime>();
        beforeState.Add("dummy", now.AddSeconds(-1));

        var afterState = new Dictionary<string, DateTime>();
        afterState.Add("dummy", now);

        var before = new FolderState("DummyPath", beforeState);
        var after = new FolderState("DummyPath", afterState);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;
        FolderUpdateChecker.Check(before, after, logger);

        Assert.Single(logger.Logs);
        Assert.Equal("File dummy was updated after the last capture.", logger.Logs.First().Message);
    }

    [Fact]
    public void ThrowParadoxTest()
    {
        var now = DateTime.UtcNow;
        var beforeState = new Dictionary<string, DateTime>();
        beforeState.Add("dummy", now);

        var afterState = new Dictionary<string, DateTime>();
        afterState.Add("dummy", now.AddSeconds(-1));

        var before = new FolderState("DummyPath", beforeState);
        var after = new FolderState("DummyPath", afterState);

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = (factory.CreateLogger<FolderUpdateCheckerTest>() as TestOutputLogger<FolderUpdateCheckerTest>)!;

        Assert.Throws<InvalidOperationException>(() => FolderUpdateChecker.Check(before, after, logger));
    }
}
