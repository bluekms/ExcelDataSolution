using CommandLine;
using Serilog.Events;

namespace StaticDataHeaderGenerator.ProgramOptions;

[Verb("all-header", HelpText = "Generate StaticData Header Names")]
public class GenerateAllHeaderOptions
{
    [Option('r', "record-path", Required = true, HelpText = "C# 레코드 파일 경로")]
    public string RecordCsPath { get; set; } = null!;

    [Option('i', "length-ini-path", Required = true, HelpText = "Length Template Ini 파일 경로")]
    public string LengthIniPath { get; set; } = null!;

    [Option('s', "separator", Default = "\t", Required = false, HelpText = "Header 구분자. 기본값: Tab")]
    public string Separator { get; set; } = null!;

    [Option('o', "output-file", Required = false, HelpText = "Header Name 파일 출력 경로. 없다면 콘솔에 출력")]
    public string OutputFileName { get; set; } = null!;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 경로")]
    public string? LogPath { get; set; }

    [Option('v', Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)")]
    public LogEventLevel MinLogLevel { get; set; }
}
