using CommandLine;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace ExcelColumnExtractor;

public sealed class ProgramOptions
{
    [Option('c', "class-path", Required = true, HelpText = "C# 클래스 파일 경로")]
    public string ClassPath { get; set; } = null!;

    [Option('e', "excel-path", Required = true, HelpText = "액셀 파일 경로")]
    public string ExcelPath { get; set; } = null!;

    [Option('o', "output-path", Required = true, HelpText = "출력 파일 경로")]
    public string OutputPath { get; set; } = null!;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 경로")]
    public string? LogPath { get; set; }

    [Option(Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (기본값: Information)")]
    public LogEventLevel MinLogLevel { get; set; }
}
