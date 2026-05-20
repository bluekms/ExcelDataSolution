using System.Collections;
using System.Collections.Immutable;

namespace Sdp.Table;

public abstract class StaticDataTable<TSelf, TRecord>(ImmutableArray<TRecord> records)
    : IStaticDataTable
    where TSelf : StaticDataTable<TSelf, TRecord>
    where TRecord : notnull
{
    public ImmutableArray<TRecord> Records => records;

    protected virtual void Validate()
    {
    }

    Type IStaticDataTable.RecordType => typeof(TRecord);

    IEnumerable IStaticDataTable.GetAllRecords() => records;

    void IStaticDataTable.Validate() => Validate();
}
