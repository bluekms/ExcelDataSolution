using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnitTest.Utility;

public static class TypeConverter
{
    public static Type GetSystemTypeName(string typeName)
    {
        var isNullable = typeName.EndsWith('?');
        if (isNullable)
        {
            typeName = typeName.TrimEnd('?');
        }

        var systemType = typeName.ToLowerInvariant() switch
        {
            "bool" => typeof(bool),
            "char" => typeof(char),
            "sbyte" => typeof(sbyte),
            "byte" => typeof(byte),
            "short" => typeof(short),
            "ushort" => typeof(ushort),
            "int" => typeof(int),
            "uint" => typeof(uint),
            "long" => typeof(long),
            "ulong" => typeof(ulong),
            "float" => typeof(float),
            "double" => typeof(double),
            "decimal" => typeof(decimal),
            "string" => typeof(string),
            _ => throw new ArgumentException($"Invalid type name: {typeName}")
        };

        if (isNullable && systemType != typeof(string))
        {
            systemType = typeof(Nullable<>).MakeGenericType(systemType);
        }

        return systemType;
    }
}
