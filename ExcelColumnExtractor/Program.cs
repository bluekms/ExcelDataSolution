using CommandLine;
using ExcelColumnExtractor.Checkers;
using ExcelColumnExtractor.NameObjects;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using StaticDataAttribute;

namespace ExcelColumnExtractor;

public class Program
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<ProgramOptions>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
    }

    private static void RunOptions(ProgramOptions options)
    {
        var logger = Logger.CreateLogger(options);

        var recordSchemaContainer = RecordScanner.Scan(options.ClassPath, logger);
        var sheetNameContainer = SheetScanner.Scan(options.ExcelPath, logger);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values.OrderBy(x => x.RecordName.FullName))
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            var sheetName = new SheetName(
                recordSchema.GetAttributeValue<StaticDataRecordAttribute, string>(0),
                recordSchema.GetAttributeValue<StaticDataRecordAttribute, string>(1));
            if (!sheetNameContainer.ContainsKey(sheetName.FullName))
            {
                LogWarning(logger, $"Not found sheet {sheetName.FullName}.", null);
            }

            var sheetHeaders = HeaderScanner.Scan(sheetNameContainer[sheetName.FullName], logger);
            LogTrace(logger, $"{sheetName.FullName}: {string.Join(", ", sheetHeaders)}", null);

            HeaderChecker.Check(recordSchema.RecordParameterSchemaList, recordSchemaContainer, sheetHeaders, sheetName);
        }
    }

    private static void HandleParseError(IEnumerable<Error> errors)
    {
        Console.WriteLine($"Errors {errors.Count()}");
        foreach (var error in errors)
        {
            Console.WriteLine(error.ToString());
        }
    }
}
