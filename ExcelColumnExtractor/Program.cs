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

        var sw = Stopwatch.StartNew();
        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        var staticDataRecordSchemaList = recordSchemaContainer.GetStaticDataRecordSchemata();
        if (staticDataRecordSchemaList.Count == 0)
        {
            var exception = new ArgumentException($"{nameof(StaticDataRecordAttribute)} is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(RecordScanner), null);

        sw.Restart();
        var sheetNameContainer = SheetNameScanner.Scan(options.ExcelPath, logger);
        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(SheetNameScanner), null);

        sw.Restart();
        var targetColumnIndicesContainer = RequiredHeadersChecker.Check(
            staticDataRecordSchemaList,
            recordSchemaContainer,
            sheetNameContainer,
            logger);
        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(RequiredHeadersChecker), null);

        sw.Restart();
        var extractedTableContainer = BodyColumnAggregator.Aggregate(
            staticDataRecordSchemaList,
            sheetNameContainer,
            targetColumnIndicesContainer,
            logger);
        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(BodyColumnAggregator), null);

        sw.Restart();
        DataBodyChecker.Check(
            staticDataRecordSchemaList,
            recordSchemaContainer,
            extractedTableContainer,
            logger);
        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(DataBodyChecker), null);

        sw.Restart();
        CsvWriter.Write(
            CheckAndCreateOutputDirectory(options, logger),
            ParseEncoding(options.Encoding),
            extractedTableContainer);
        LogInformation(logger, sw.Elapsed.TotalMilliseconds, nameof(CsvWriter), null);
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
        else
        {
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
        }

        return path;
    }

    private static void HandleParseError(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();

        Console.WriteLine($"Errors {errorList.Count}");
        foreach (var error in errorList)
        {
            Console.WriteLine(error.ToString());
        }
    }

    private static readonly Action<ILogger, double, string, Exception?> LogInformation =
        LoggerMessage.Define<double, string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "Complete({Ms:00.0000}ms) {Work}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
