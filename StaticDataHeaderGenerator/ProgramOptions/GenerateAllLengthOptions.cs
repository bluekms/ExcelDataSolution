using CommandLine;
using Serilog.Events;

namespace StaticDataHeaderGenerator.ProgramOptions;

[Verb("all-length", HelpText = "Generate an ini file to take input for the length needed to create the header.")]
public class GenerateAllLengthOptions
{
    [Option('r', "record-path", Required = true, HelpText = "C# 레코드 파일 경로")]
    public string RecordCsPath { get; set; } = null!;

    [Option('i', "ini-path", Required = true, HelpText = "생성할 ini 파일 경로")]
    public string OutputPath { get; set; } = null!;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 출력 경로")]
    public string? LogPath { get; set; }

    [Option('v', Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)")]
    public LogEventLevel MinLogLevel { get; set; }
}
