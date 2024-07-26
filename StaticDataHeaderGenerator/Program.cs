using CLICommonLibrary;
using CommandLine;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace StaticDataHeaderGenerator;

internal class Program
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<GenerateHeaderOptions, GenerateLengthTemplateOptions>(args)
            .MapResult(
                (GenerateHeaderOptions options) => GenerateHeader(options),
                (GenerateLengthTemplateOptions options) => GenerateLengthTemplate(options),
                HandleParseError);
    }

    private static int GenerateLengthTemplate(GenerateLengthTemplateOptions options)
    {
        Console.WriteLine("GenerateLengthTemplate");
        var logger = Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);
        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        return 0;
    }

    private static int GenerateHeader(GenerateHeaderOptions options)
    {
        Console.WriteLine("GenerateHeader");
        var logger = Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);
        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values.OrderBy(x => x.RecordName.FullName))
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            var lengthRequiredNames = recordSchema.FindLengthRequiredNames(recordSchemaContainer);
        }

        return 0;
    }

    private static int HandleParseError(IEnumerable<Error> errors)
    {
        Console.WriteLine($"Errors {errors.Count()}");
        foreach (var error in errors)
        {
            Console.WriteLine(error.ToString());
        }

        return 1;
    }
}
