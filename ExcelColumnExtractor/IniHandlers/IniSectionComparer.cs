namespace ExcelColumnExtractor.IniHandlers;

public class IniSectionComparer
{
    public enum Result
    {
        Unchanged,
        Modified,
    }

    public static Result Compare(RecordContainerInfo oldInfo, RecordContainerInfo newInfo)
    {
        if (oldInfo.RecordName != newInfo.RecordName)
        {
            throw new ArgumentException("Record names do not match.");
        }

        return !oldInfo.LengthRequiredHeaderNames.SetEquals(newInfo.LengthRequiredHeaderNames)
            ? Result.Modified
            : Result.Unchanged;
    }
}
