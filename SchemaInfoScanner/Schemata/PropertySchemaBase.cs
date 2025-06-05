using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public abstract record PropertySchemaBase(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    protected abstract int OnCheckCompatibility(CompatibilityContext context, ILogger logger);

    public int CheckCompatibility(CompatibilityContext context, ILogger logger)
    {
        try
        {
            return OnCheckCompatibility(context, logger);
        }
        catch (Exception e)
        {
            LogError(logger, GetType(), context.ToString(), e, e.InnerException);
            throw;
        }
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
