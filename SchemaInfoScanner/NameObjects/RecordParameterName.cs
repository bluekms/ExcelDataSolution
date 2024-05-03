using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.NameObjects;

public class RecordParameterName
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
        RecordName = new RecordName(string.Join('.', parts[..^1]));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        return FullName == ((RecordParameterName)obj).FullName;
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}
