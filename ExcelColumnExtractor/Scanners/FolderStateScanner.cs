namespace ExcelColumnExtractor.Scanners;

public static class FolderStateScanner
{
    public static FolderState Scan(string folderPath, params string[] extensions)
    {
        var fileStates = Directory.GetFiles(folderPath)
            .Where(x => extensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
            .ToDictionary(
                x => Path.GetFileName(x),
                File.GetLastWriteTimeUtc);

        return new FolderState(folderPath, fileStates);
    }
}
