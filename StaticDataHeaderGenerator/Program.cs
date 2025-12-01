using CommandLine;
using StaticDataHeaderGenerator.OptionHandlers;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator;

internal class Program
{
    private static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<
                GenerateHeaderOptions,
                GenerateAllHeaderOptions>(args)
            .MapResult(
                (GenerateHeaderOptions options) => GenerateHeaderHandler.Generate(options),
                (GenerateAllHeaderOptions options) => GenerateAllHeaderHandler.Generate(options),
                HandleParseError);
    }

    private static int HandleParseError(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();

        Console.WriteLine($"Errors {errorList.Count}");
        foreach (var error in errorList)
        {
            Console.WriteLine(error.ToString());
        }

        return 1;
    }
}
