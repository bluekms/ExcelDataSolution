using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner;

public class RecordParameterName
{
    private readonly RecordName recordName;
    private readonly ParameterSyntax parameterSyntax;

    public RecordName RecordName => recordName;

    public string Name => parameterSyntax.Identifier.ValueText;

    public string FullName => $"{recordName}.{parameterSyntax.Identifier.ValueText}";

    public RecordParameterName(RecordName recordName, ParameterSyntax parameterSyntax)
    {
        this.recordName = recordName;
        this.parameterSyntax = parameterSyntax;
    }

    public override string ToString()
    {
        return Name;
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
