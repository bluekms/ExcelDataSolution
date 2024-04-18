using CommandLine;

namespace ExcelColumnExtractor;

public sealed class ProgramOptions
{
    [Option('c', "class-path", Required = true, HelpText = "C# 클래스 파일 경로")]
    public string ClassPath { get; set; } = null!;

    [Option('e', "excel-path", Required = true, HelpText = "액셀 파일 경로")]
    public string ExcelPath { get; set; } = null!;

    [Option('o', "output", Required = false, HelpText = "출력 파일 경로")]
    public string? OutputPath { get; set; }
}
