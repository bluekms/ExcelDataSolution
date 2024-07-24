namespace StaticDataAttribute;

/// <summary>
/// This Attribute only affects the output of RecordSchemaFlattener.Flattener()
/// It has no effect on running code.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RecordGlobalIndexingModeAttribute : Attribute
{
    public IndexingMode Mode { get; }

    public RecordGlobalIndexingModeAttribute(IndexingMode mode)
    {
        Mode = mode;
    }
}
