namespace ExcelColumnExtractor.Scanners;

public sealed record FolderState(
    string FolderPath,
    IReadOnlyDictionary<string, DateTime> FileStates);
