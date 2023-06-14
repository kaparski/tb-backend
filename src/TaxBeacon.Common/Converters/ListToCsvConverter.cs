using Npoi.Mapper.Attributes;
using System.Reflection;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Converters;

public sealed class ListToCsvConverter: IListToFileConverter
{
    public FileType FileType => FileType.Csv;

    public byte[] Convert<T>(List<T> data)
    {
        using var stream = new MemoryStream();
        using var sw = new StreamWriter(stream);
        CreateHeader<T>(sw);
        CreateRows(data, sw);

        sw.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        return stream.ToArray();
    }

    private static void CreateHeader<T>(StreamWriter sw)
    {
        var properties = typeof(T).GetProperties()
                                  .Where(x => x.GetCustomAttribute<IgnoreAttribute>() is null)
                                  .ToArray();
        for (var i = 0; i < properties.Length - 1; i++)
        {
            var property = properties[i];
            var columnAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false)
                                           .FirstOrDefault() as ColumnAttribute;
            sw.Write((columnAttribute?.Name ?? property.Name) + ",");
        }
        var lastProp = properties[^1];
        var lastPropColumnAttribute = lastProp.GetCustomAttributes(typeof(ColumnAttribute), false)
                           .FirstOrDefault() as ColumnAttribute;
        sw.Write((lastPropColumnAttribute?.Name ?? lastProp.Name) + Environment.NewLine);
    }

    private static void CreateRows<T>(List<T> list, StreamWriter sw)
    {
        var properties = typeof(T).GetProperties()
                      .Where(x => x.GetCustomAttribute<IgnoreAttribute>() is null)
                      .ToArray();
        foreach (var item in list)
        {
            for (var i = 0; i < properties.Length - 1; i++)
            {
                var property = properties[i];
                ProcessRow(sw, item, property);
            }
            var lastProp = properties[^1];
            ProcessRow(sw, item, lastProp, true);
        }
    }

    private static void ProcessRow<T>(StreamWriter sw, T? item, PropertyInfo property, bool isLast = false)
    {
        var columnAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false)
                   .FirstOrDefault() as ColumnAttribute;

        if (string.IsNullOrEmpty(columnAttribute?.CustomFormat))
        {
            sw.Write($"\"{property.GetValue(item)}\"" + (!isLast ? "," : Environment.NewLine));
        }
        else
        {
            if (property.PropertyType == typeof(DateTime))
            {
                sw.Write(((DateTime)property.GetValue(item)!).ToString(columnAttribute.CustomFormat.Replace("AM/PM", "tt")) + (!isLast ? "," : Environment.NewLine));
            }
            else if (property.PropertyType == typeof(DateTime?))
            {
                sw.Write(((DateTime?)property.GetValue(item))?.ToString(columnAttribute.CustomFormat.Replace("AM/PM", "tt")) + (!isLast ? "," : Environment.NewLine));
            }
            else
            {
                sw.Write(property.GetValue(item) + (!isLast ? "," : Environment.NewLine));
            }
        }
    }
}
