using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public abstract record PropertySchemaBase(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    public override string ToString()
    {
        return PropertyName.FullName;
    }

    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer, ILogger logger)
    {
        var enumerator = Enumerable.Repeat(argument, 1).GetEnumerator();
        CheckCompatibility(enumerator, enumMemberContainer, logger);
    }

    public void CheckCompatibility(IEnumerator<string> arguments, EnumMemberContainer enumMemberContainer, ILogger logger)
    {
        try
        {
            OnCheckCompatibility(arguments, enumMemberContainer, logger);
        }
        catch (Exception e)
        {
            LogError(logger, GetType(), arguments.Current, e, e.InnerException);
            throw;
        }
    }

    protected static string GetNextArgument(IEnumerator<string> arguments, Type schemaType, ILogger logger)
    {
        if (!arguments.MoveNext())
        {
            var ex = new InvalidOperationException("Column count error.");
            LogError(logger, schemaType, ex.Message, ex, ex.InnerException);
            throw ex;
        }

        return arguments.Current;
    }

    protected abstract void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger);

    protected static readonly Action<ILogger, Type, string, Exception?, Exception?> LogError =
        LoggerMessage.Define<Type, string, Exception?>(
            LogLevel.Error,
            new EventId(0, nameof(PropertySchemaBase)),
            "Type: {Type}, Argument: {Argument} is not compatible. InnerException: {InnerException}");
}
