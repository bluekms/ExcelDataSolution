using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public abstract record ParameterSchemaBase(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    public override string ToString()
    {
        return ParameterName.FullName;
    }

    public void CheckCompatibility(string argument, ILogger logger)
    {
        try
        {
            OnCheckCompatibility(argument, logger);
        }
        catch (Exception e)
        {
            LogError(logger, GetType(), argument, e);
        }
    }

    protected abstract void OnCheckCompatibility(string argument, ILogger logger);

    private static readonly Action<ILogger, Type, string, Exception?> LogError =
        LoggerMessage.Define<Type, string>(
            LogLevel.Error,
            new EventId(0, nameof(ParameterSchemaBase)),
            "Type: {Type}, Argument: {Argument} is not compatible");
}
