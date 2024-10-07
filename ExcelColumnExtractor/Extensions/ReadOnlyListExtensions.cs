namespace ExcelColumnExtractor.Extensions;

public static class ReadOnlyListExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
    {
        return list.Select((value, index) => new { Value = value, Index = index })
            .FirstOrDefault(x => EqualityComparer<T>.Default.Equals(x.Value, item))
            ?.Index ?? -1;
    }
}
