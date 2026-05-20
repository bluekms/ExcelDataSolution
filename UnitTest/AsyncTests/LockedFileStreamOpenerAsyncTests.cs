using ExcelColumnExtractor.Scanners;
using UnitTest.Utility;

namespace UnitTest.AsyncTests;

[Collection("ExcelFileTests")]
public class LockedFileStreamOpenerAsyncTests
{
    private const string ExcelResourceFileName = "Excel1.xlsx";

    [Fact]
    public async Task CreateAsync_WithValidFile_ReturnsOpener()
    {
        using var testData = new TestDataDirectory(ExcelResourceFileName);
        var excelPath = testData.GetFilePath(ExcelResourceFileName);
        Assert.True(File.Exists(excelPath), $"Test file not found: {excelPath}");

        using var opener = await LockedFileStreamOpener.CreateAsync(excelPath);

        Assert.NotNull(opener);
        Assert.NotNull(opener.Stream);
        Assert.True(opener.Stream.CanRead);
    }

    [Fact]
    public async Task CreateAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var testData = new TestDataDirectory(ExcelResourceFileName);
        var excelPath = testData.GetFilePath(ExcelResourceFileName);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            LockedFileStreamOpener.CreateAsync(excelPath, cts.Token));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var invalidPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}.xlsx");

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            LockedFileStreamOpener.CreateAsync(invalidPath));
    }

    [Fact]
    public async Task CreateAsync_StreamDisposesCorrectly()
    {
        using var testData = new TestDataDirectory(ExcelResourceFileName);
        var excelPath = testData.GetFilePath(ExcelResourceFileName);

        var opener = await LockedFileStreamOpener.CreateAsync(excelPath);
        var stream = opener.Stream;
        opener.Dispose();

        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    [Fact]
    public async Task CreateAsync_IsTemp_IsFalseForUnlockedFile()
    {
        using var testData = new TestDataDirectory(ExcelResourceFileName);
        var excelPath = testData.GetFilePath(ExcelResourceFileName);
        using var opener = await LockedFileStreamOpener.CreateAsync(excelPath);

        Assert.False(opener.IsTemp);
    }
}
