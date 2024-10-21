using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata;

// TODO RawRecordSchema가 필요없을것 같음. 이걸 하는곳은 DataBodyChecker 이기 때문 여기는 length정보도 있음
public static class RecordSchemaFactory
{
    public static RecordSchema Create(RawRecordSchema schema, EnumMemberContainer enumMemberContainer)
    {
        var parameterList = new List<ParameterSchemaBase>();
        foreach (var parameter in schema.RawParameterSchemaList)
        {
            var list = Process(parameter);
            parameterList.AddRange(list);
        }

        return new(
            schema.RecordName,
            schema.NamedTypeSymbol,
            schema.RecordAttributeList,
            parameterList);
    }

    private static IReadOnlyList<ParameterSchemaBase> Process(RawParameterSchema parameter)
    {
        var list = new List<ParameterSchemaBase>();

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
        {
            list.Add(TypedParameterSchemaFactory.Create(parameter));
        }
        else if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
        {
            // length 정보가 필요함
        }
        else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
        {
        }
        else if (RecordTypeChecker.IsSupportedRecordType(parameter.NamedTypeSymbol))
        {
        }
        else
        {
            throw new TypeNotSupportedException($"{parameter.NamedTypeSymbol.Name} is not supported type.");
        }

        return list;
    }
}
