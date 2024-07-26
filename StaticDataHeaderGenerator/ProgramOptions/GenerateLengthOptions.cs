using CommandLine;
using Serilog.Events;

namespace StaticDataHeaderGenerator.ProgramOptions;

[Verb("length", HelpText = "Generate an ini file to take input for the length needed to create the header.")]
public sealed class GenerateLengthOptions
{
    [Option('r', "record-path", Required = true, HelpText = "C# 레코드 파일 경로")]
    public string RecordCsPath { get; set; } = null!;

    [Option('n', "record-name", Required = true, HelpText = "레코드 이름")]
    public string RecordName { get; set; } = null!;

    [Option('o', "output-path", Required = true, HelpText = "생성할 ini 파일 출력 경로")]
    public string OutputPath { get; set; } = null!;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 경로")]
    public string? LogPath { get; set; }

    [Option('v', Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)")]
    public LogEventLevel MinLogLevel { get; set; }
}
