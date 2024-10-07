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
            ForceCopyAsync(fileName, TempFileName);
            Stream = File.Open(TempFileName, FileMode.Open, FileAccess.Read);
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

    private static void ForceCopyAsync(string src, string dst)
    {
        var command = $"COPY /B /Y {src} {dst}";
        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C {command}",
        };
        process.EnableRaisingEvents = true;
        process.Start();
        process.WaitForExit();
    }
}
