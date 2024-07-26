using CommandLine;
using Serilog.Events;

namespace StaticDataHeaderGenerator;

[Verb("template", HelpText = "GenerateHeader를 수행하는데 필요한 Length Template 을 생성.")]
public sealed class GenerateLengthTemplateOptions
{
    [Option('r', "record-path", Required = true, HelpText = "C# 레코드 파일 경로")]
    public string RecordCsPath { get; set; } = null!;

    [Option('o', "output-path", Required = true, HelpText = "출력 Length Template Ini 파일 경로")]
    public string OutputPath { get; set; } = null!;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 경로")]
    public string? LogPath { get; set; }

    [Option('v', Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)")]
    public LogEventLevel MinLogLevel { get; set; }
}
