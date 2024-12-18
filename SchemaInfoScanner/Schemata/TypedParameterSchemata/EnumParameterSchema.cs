using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record EnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(string argument, ILogger logger)
    {
        throw new InvalidOperationException($"{ParameterName.FullName} is enum. Use CheckCompatibility(string, EnumMemberContainer, ILogger) instead");
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer, ILogger logger)
    {
        var enumName = new EnumName(NamedTypeSymbol.Name);
        var enumMembers = enumMemberContainer.GetEnumMembers(enumName);
        if (!enumMembers.Contains(argument))
        {
            var ex = new InvalidOperationException($"{argument} is not a member of {enumName.FullName}");
            LogError(logger, argument, enumName.FullName, ex);
        }
    }

    private static readonly Action<ILogger, string, string, Exception?> LogError =
        LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Argument} is not a member of {EnumFullName}");
}
