namespace Sdp.View;

public abstract class StaticDataView<TSelf, TTableSet>(TTableSet tables)
    where TSelf : StaticDataView<TSelf, TTableSet>
    where TTableSet : class
{
    protected TTableSet Tables => tables;

    protected virtual void Validate()
    {
    }
}
