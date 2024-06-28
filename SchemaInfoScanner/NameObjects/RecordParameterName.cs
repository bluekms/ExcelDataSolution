using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.NameObjects;

public class RecordParameterName : IEquatable<RecordParameterName>
{
    public string Name { get; }
    public string FullName => $"{RecordName.FullName}.{Name}";

    public RecordName RecordName { get; }

    public RecordParameterName(RecordName recordName, ParameterSyntax parameterSyntax)
    {
        Name = parameterSyntax.Identifier.ValueText;
        RecordName = recordName;
    }

    public RecordParameterName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName) || fullName[^1] == '.')
        {
            throw new ArgumentException("fullName should not be null, empty, or end with '.'");
        }

        var parts = fullName.Split('.');
        Name = parts[^1];

        var namespacePart = string.Join('.', parts[..^1]);
        RecordName = string.IsNullOrEmpty(namespacePart)
            ? new RecordName(Name)
            : new RecordName(namespacePart);
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

        return Equals((RecordParameterName)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.RecordName);
    }

    public bool Equals(RecordParameterName? other)
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
