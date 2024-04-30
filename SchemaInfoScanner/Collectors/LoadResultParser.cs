namespace SchemaInfoScanner.Collectors;

public sealed partial class RecordSchemaCollector
{
    private static class LoadResultParser
    {
        public static Result Parse(LoadResult loadResult)
        {
            var recordAttributeCollector = new RecordAttributeCollector();
            var parameterAttributeCollector = new ParameterAttributeCollector();
            var parameterNamedTypeSymbolCollector = new ParameterNamedTypeSymbolCollector(loadResult.SemanticModel);
            var enumMemberCollector = new EnumMemberCollector();

            foreach (var recordDeclaration in loadResult.RecordDeclarationList)
            {
                recordAttributeCollector.Collect(recordDeclaration);

                if (recordDeclaration.ParameterList is null)
                {
                    continue;
                }

                foreach (var parameter in recordDeclaration.ParameterList.Parameters)
                {
                    if (string.IsNullOrEmpty(parameter.Identifier.ValueText))
                    {
                        continue;
                    }

                    parameterAttributeCollector.Collect(recordDeclaration, parameter);
                    parameterNamedTypeSymbolCollector.Collect(recordDeclaration, parameter);
                }
            }

            foreach (var enumDeclaration in loadResult.EnumDeclarationList)
            {
                enumMemberCollector.Collect(enumDeclaration);
            }

            return new(
                recordAttributeCollector,
                parameterAttributeCollector,
                parameterNamedTypeSymbolCollector,
                enumMemberCollector);
        }

        public sealed class Result
        {
            public RecordAttributeCollector RecordAttributeCollector { get; }
            public ParameterAttributeCollector ParameterAttributeCollector { get; }
            public ParameterNamedTypeSymbolCollector ParameterNamedTypeSymbolCollector { get; }
            public EnumMemberCollector EnumMemberCollector { get; }

            public Result(
                RecordAttributeCollector recordAttributeCollector,
                ParameterAttributeCollector parameterAttributeCollector,
                ParameterNamedTypeSymbolCollector parameterNamedTypeSymbolCollector,
                EnumMemberCollector enumMemberCollector)
            {
                if (parameterNamedTypeSymbolCollector.Count != parameterAttributeCollector.Count)
                {
                    throw new ArgumentException("Count mismatch");
                }

                foreach (var parameterFullName in parameterNamedTypeSymbolCollector.ParameterNames)
                {
                    if (!parameterAttributeCollector.ContainsRecord(parameterFullName))
                    {
                        throw new ArgumentException($"{parameterFullName} not found");
                    }
                }

                RecordAttributeCollector = recordAttributeCollector;
                ParameterAttributeCollector = parameterAttributeCollector;
                ParameterNamedTypeSymbolCollector = parameterNamedTypeSymbolCollector;
                EnumMemberCollector = enumMemberCollector;
            }
        }
    }
}
