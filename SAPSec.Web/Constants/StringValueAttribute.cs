using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Constants;

[ExcludeFromCodeCoverage]
public class StringValueAttribute(string value) : Attribute
{
    public string StringValue { get; protected set; } = value;
}

public static class EnumExtensions
{
    public static string GetStringValue(this Enum value)
    {
        var type = value.GetType();
        var fieldInfo = type.GetField(value.ToString());
        var attribs = fieldInfo?
            .GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

        return attribs is null || attribs.Length == 0 ? "" : attribs[0].StringValue;
    }
}