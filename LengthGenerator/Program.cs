using CommandLine;
using LengthGenerator.OptionHandlers;
using LengthGenerator.ProgramOptions;

namespace LengthGenerator;

internal class Program
{
    private static void Main(string[] args) =>
        Parser.Default.ParseArguments<
                GenLengthOptions>(args)
            .MapResult(
                (GenLengthOptions options) => GenLengthHandler.Generate(options),
                HandleParseError);

    private static int HandleParseError(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();

        Console.WriteLine($@"Errors {errorList.Count}");
        foreach (var error in errorList)
        {
            Console.WriteLine(error.ToString());
        }

        return 1;
    }
}
