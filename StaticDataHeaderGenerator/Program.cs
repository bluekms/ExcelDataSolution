using CommandLine;
using StaticDataHeaderGenerator.OptionHandlers;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator;

internal class Program
{
    private static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<GenerateHeaderOptions, GenerateLengthFileOptions>(args)
            .MapResult(
                (GenerateLengthFileOptions options) => GenerateLengthFileHandler.Generate(options),
                (GenerateAllLengthTemplateOptions options) => GenerateAllLengthFileHandler.Generate(options),
                (GenerateHeaderOptions options) => GenerateHeaderHandler.Generate(options),
                (GenerateAllHeaderOptions options) => GenerateAllHeaderHandler.Generate(options),
                HandleParseError);
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
