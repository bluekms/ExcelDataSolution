using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedParameterSchemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata;

// TODO RawRecordSchema가 필요없을것 같음. 이걸 하는곳은 DataBodyChecker 이기 때문 여기는 length정보도 있음
public static class RecordSchemaFactory
{
    public static RecordSchema Create(
        RawRecordSchema schema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths)
    {
        var parameterList = new List<ParameterSchemaBase>();
        foreach (var parameter in schema.RawParameterSchemaList)
        {
            var list = Process(parameter, recordSchemaContainer, headerLengths);
            parameterList.AddRange(list);
        }

        return new(
            schema.RecordName,
            schema.NamedTypeSymbol,
            schema.RecordAttributeList,
            parameterList);
    }

    private static ReadOnlyCollection<ParameterSchemaBase> Process(
        RawParameterSchema parameter,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths)
    {
        var list = new List<ParameterSchemaBase>();

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
        {
            list.Add(TypedParameterSchemaFactory.Create(parameter));
        }
        else if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
        {
            var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
            var innerParameterSchema = TypedParameterSchemaFactory.Create(
                parameter.ParameterName,
                typeArgument,
                parameter.AttributeList);

            if (parameter.HasAttribute<SingleColumnContainerAttribute>())
            {
                var separator = parameter.GetAttributeValue<SingleColumnContainerAttribute, string>();

                list.Add(new SingleColumnContainerParameterSchema(
                    parameter.ParameterName,
                    parameter.NamedTypeSymbol,
                    parameter.AttributeList,
                    separator,
                    innerParameterSchema));
            }
            else
            {
                var length = headerLengths[parameter.ParameterName.Name];
                list.AddRange(Enumerable.Repeat(innerParameterSchema, length));
            }
        }
        else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
        {
            var innerRecordSchema = parameter.FindInnerRecordSchema(recordSchemaContainer);

            var innerList = new List<ParameterSchemaBase>();
            foreach (var innerRecordParameter in innerRecordSchema.RawParameterSchemaList)
            {
                var result = Process(innerRecordParameter, recordSchemaContainer, headerLengths);
                innerList.AddRange(result);
            }

            var length = headerLengths[parameter.ParameterName.Name];
            list.AddRange(Enumerable.Repeat(innerList, length).SelectMany(x => x));
        }
        else if (RecordTypeChecker.IsSupportedRecordType(parameter.NamedTypeSymbol))
        {
            throw new NotImplementedException();
        }
        else
        {
            throw new TypeNotSupportedException($"{parameter.NamedTypeSymbol.Name} is not supported type.");
        }

        return list.AsReadOnly();
    }
}
