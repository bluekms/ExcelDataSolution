using System.Diagnostics;
using System.Globalization;
using System.Text;
using CLICommonLibrary;
using CommandLine;
using ExcelColumnExtractor.Aggregator;
using ExcelColumnExtractor.Checkers;
using ExcelColumnExtractor.Scanners;
using ExcelColumnExtractor.Writers;
using Microsoft.Extensions.Logging;
using StaticDataAttribute;

namespace ExcelColumnExtractor;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<ProgramOptions>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
    }

    private static void RunOptions(ProgramOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        var beforeCsState = FolderStateScanner.Scan(options.RecordCsPath, ".cs");
        var beforeExcelState = FolderStateScanner.Scan(options.ExcelPath, ".xls", ".xlsx");

        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var recordSchemaCatalog = RecordScanner.Scan(options.RecordCsPath, logger);
        if (recordSchemaCatalog.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException($"{nameof(StaticDataRecordAttribute)} is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(RecordScanner), null);

        sw.Restart();
        var sheetNameMap = SheetNameScanner.Scan(options.ExcelPath, logger);
        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(SheetNameScanner), null);

        var targetColumnIndicesCollection = RequiredHeadersChecker.Check(
            recordSchemaCatalog.StaticDataRecordSchemata,
            recordSchemaCatalog,
            sheetNameMap,
            logger);
        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(RequiredHeadersChecker), null);

        sw.Restart();
        var extractedTableCollection = BodyColumnAggregator.Aggregate(
            recordSchemaCatalog.StaticDataRecordSchemata,
            sheetNameMap,
            targetColumnIndicesCollection,
            logger);
        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(BodyColumnAggregator), null);

        sw.Restart();

        DataBodyChecker.Check(
            recordSchemaCatalog.StaticDataRecordSchemata,
            recordSchemaCatalog,
            extractedTableCollection,
            logger);
        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(DataBodyChecker), null);

        sw.Restart();
        CsvWriter.Write(
            CheckAndCreateOutputDirectory(options, logger),
            ParseEncoding(options.Encoding),
            extractedTableCollection);
        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(CsvWriter), null);

        sw.Restart();
        var afterCsState = FolderStateScanner.Scan(options.RecordCsPath, ".cs");
        var afterExcelState = FolderStateScanner.Scan(options.ExcelPath, ".xls", ".xlsx");

        FolderUpdateChecker.Check(beforeCsState, afterCsState, logger);
        FolderUpdateChecker.Check(beforeExcelState, afterExcelState, logger);

        LogTrace(logger, sw.Elapsed.TotalMilliseconds, nameof(FolderStateScanner), null);
        LogInformation(logger, totalSw.Elapsed.TotalMilliseconds, nameof(ExcelColumnExtractor), null);
    }

    private static Encoding ParseEncoding(string? encoding)
    {
        var encodingName = (string.IsNullOrEmpty(encoding) ? "UTF-8" : encoding)
            .ToUpper(CultureInfo.InvariantCulture);

        return encodingName switch
        {
            "UTF-8" => new UTF8Encoding(false),
            "UTF-16" => Encoding.Unicode,
            "UTF-32" => Encoding.UTF32,
            "ASCII" => Encoding.ASCII,
            _ => Encoding.GetEncoding(encodingName),
        };
    }

    private static string CheckAndCreateOutputDirectory(ProgramOptions options, ILogger<Program> logger)
    {
        var path = string.IsNullOrEmpty(options.Version)
            ? options.OutputPath
            : Path.Combine(options.OutputPath, options.Version);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (options.Version is not null &&
            !options.Version.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            var fileCount = Directory.GetFiles(path).Length;
            if (fileCount > 0)
            {
                var exception = new ArgumentException($"The directory already exists and contains {fileCount} files.");
                LogError(logger, exception.Message, exception);
                throw exception;
            }
        }

        return path;
    }

    private static void HandleParseError(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();

        Console.WriteLine($@"Errors {errorList.Count}");
        foreach (var error in errorList)
        {
            Console.WriteLine(error.ToString());
        }
    }

    private static readonly Action<ILogger, double, string, Exception?> LogTrace =
        LoggerMessage.Define<double, string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "Complete({Ms:00.0000}ms) {Work}");

    private static readonly Action<ILogger, double, string, Exception?> LogInformation =
        LoggerMessage.Define<double, string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "Complete({Ms:00.0000}ms) {Work}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
