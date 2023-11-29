using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaxBeacon.Common.Converters;

public class SmartEnumConverterFactory: JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.BaseType is not null
        && typeToConvert.BaseType.IsGenericType
        && (typeToConvert.BaseType.GetGenericTypeDefinition() == typeof(SmartEnum<,>)
            || typeToConvert.BaseType.GetGenericTypeDefinition() == typeof(SmartEnum<>));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.BaseType!.GetGenericTypeDefinition() == typeof(SmartEnum<>)
            ? typeToConvert.BaseType.BaseType!.GetGenericArguments()
            : typeToConvert.BaseType.GetGenericArguments();
        var converterType = typeof(SmartEnumNameConverter<,>).MakeGenericType(typeArgs);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
