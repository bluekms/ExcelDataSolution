using CommandLine;
using Serilog.Events;

namespace LengthGenerator.ProgramOptions;

[Verb("gen-length", HelpText = "배열의 길이를 입력하는 ini  파일을 생성합니다.")]
public class GenLengthOptions
{
    [Option('r', "record-path", Required = true, HelpText = "C# 레코드 파일 경로")]
    public string RecordCsPath { get; set; } = null!;

    [Option('n', "record-name", Required = true, HelpText = "레코드 이름")]
    public string RecordName { get; set; } = null!;

    [Option('i', "ini-path", Required = true, HelpText = "생성할 ini 파일 경로")]
    public string OutputPath { get; set; } = null!;

    [Option('u', "update", Default = false, Required = false, HelpText = "기존 파일이 있을 경우 덮어쓸지 여부. 기본값: false (덮어쓰지 않음)")]
    public bool Update { get; set; } = false;

    [Option('l', "log-path", Required = false, HelpText = "로그 파일 경로")]
    public string? LogPath { get; set; }

    [Option('m', "min-log-level", Default = LogEventLevel.Information, Required = false, HelpText = "최소 로그 레벨 (Verbose, Debug, Information, Warning, Error, Fatal)")]
    public LogEventLevel MinLogLevel { get; set; }
}
