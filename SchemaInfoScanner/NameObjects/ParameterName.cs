using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.NameObjects;

public class ParameterName : IEquatable<ParameterName>
{
    public RecordName RecordName { get; }
    public string Name { get; }

    public string FullName => $"{RecordName.FullName}.{Name}";

    public ParameterName(RecordName recordName, ParameterSyntax parameterSyntax)
    {
        Name = parameterSyntax.Identifier.ValueText;
        RecordName = recordName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((ParameterName)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.RecordName);
    }

    public bool Equals(ParameterName? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Name == other.Name &&
               this.RecordName.Equals(other.RecordName);
    }

    public override string ToString()
    {
        return FullName;
    }
}
