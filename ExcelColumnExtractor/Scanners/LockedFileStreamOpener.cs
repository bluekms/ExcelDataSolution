using System.Diagnostics;

namespace ExcelColumnExtractor.Scanners;

public class LockedFileStreamOpener : IDisposable
{
    public Stream Stream { get; }
    public bool IsTemp => !string.IsNullOrEmpty(TempFileName);
    private string? TempFileName { get; }

    public LockedFileStreamOpener(string fileName)
    {
        try
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
        }
        catch (IOException)
        {
            TempFileName = Path.GetTempFileName();
            ForceCopy(fileName, TempFileName);
            Stream = File.Open(TempFileName, FileMode.Open, FileAccess.Read);
        }
    }

    private LockedFileStreamOpener(Stream stream, string? tempFileName)
    {
        Stream = stream;
        TempFileName = tempFileName;
    }

    public static async Task<LockedFileStreamOpener> CreateAsync(string fileName, CancellationToken cancellationToken = default)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        try
        {
            var stream = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);
            return new LockedFileStreamOpener(stream, null);
        }
        catch (IOException)
        {
            var tempFileName = Path.GetTempFileName();
            await ForceCopyAsync(fileName, tempFileName, cancellationToken);
            var stream = new FileStream(
                tempFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);
            return new LockedFileStreamOpener(stream, tempFileName);
        }
    }

    public void Dispose()
    {
        Stream.Dispose();
        if (!string.IsNullOrEmpty(TempFileName))
        {
            if (File.Exists(TempFileName))
            {
                File.Delete(TempFileName);
            }
        }

        GC.SuppressFinalize(this);
    }

    private static void ForceCopy(string src, string dst)
    {
        var command = $"COPY /B /Y {src} {dst}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
            },
            EnableRaisingEvents = true
        };
        process.Start();
        process.WaitForExit();
    }

    private static async Task ForceCopyAsync(string src, string dst, CancellationToken cancellationToken)
    {
        var command = $"COPY /B /Y {src} {dst}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
            },
            EnableRaisingEvents = true
        };
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
    }
}
