using CommandLine;

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
        throw new NotImplementedException();
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
