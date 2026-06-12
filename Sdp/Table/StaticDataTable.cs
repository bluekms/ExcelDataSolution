using System.Collections.Immutable;

namespace Sdp.Table;

public abstract class StaticDataTable<TSelf, TRecord>(ImmutableArray<TRecord> records)
    where TSelf : StaticDataTable<TSelf, TRecord>
    where TRecord : notnull
{
    public ImmutableArray<TRecord> Records => records;

    protected virtual void Validate()
    {
    }
}
