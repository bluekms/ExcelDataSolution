using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public abstract record ParameterSchemaBase(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    public override string ToString()
    {
        return ParameterName.FullName;
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer, ILogger logger)
    {
        try
        {
            OnCheckCompatibility(argument, enumMemberContainer, logger);
        }
        catch (Exception e)
        {
            LogError(logger, GetType(), argument, e, e.InnerException);
            throw;
        }
    }

    protected abstract void OnCheckCompatibility(string argument, EnumMemberContainer enumMemberContainer, ILogger logger);

    private static readonly Action<ILogger, Type, string, Exception?, Exception?> LogError =
        LoggerMessage.Define<Type, string, Exception?>(
            LogLevel.Error,
            new EventId(0, nameof(ParameterSchemaBase)),
            "Type: {Type}, Argument: {Argument} is not compatible. InnerException: {InnerException}");
}
