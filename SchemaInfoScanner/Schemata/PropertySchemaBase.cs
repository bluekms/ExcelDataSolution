using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata;

public abstract record PropertySchemaBase(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    protected abstract int OnCheckCompatibility(ICompatibilityContext context);

    public int CheckCompatibility(ICompatibilityContext context)
    {
        return OnCheckCompatibility(context);
    }

    public override string ToString()
    {
        return PropertyName.FullName;
    }

    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    protected static readonly Action<ILogger, Type, string, Exception?, Exception?> LogError =
        LoggerMessage.Define<Type, string, Exception?>(
            LogLevel.Error,
            new EventId(0, nameof(PropertySchemaBase)),
            "Type: {Type}, Argument: {Argument} is not compatible. InnerException: {InnerException}");
}
