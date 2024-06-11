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
        var logger = Logger.CreateLogger(options);

        var recordSchemaContainer = RecordScanner.Scan(options.ClassPath, logger);

        // TODO
        // excel 파일 불러오기
        // record에는 있지만 excel file sheet에 없는지 확인하기
        // record의 맴버를 순회하며 sheet에 있는지 확인하기
        // 맴버를 순회할 때 변환할 수 있는 타입인지 확인하기
        // 변환할 수 있다면 excel 파일에서 csv로 추출하기
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
