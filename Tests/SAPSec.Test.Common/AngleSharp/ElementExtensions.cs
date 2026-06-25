using AngleSharp.Dom;

namespace SAPSec.Test.Common.AngleSharp;

public static class ElementExtensions
{
    public static string TrimmedTextContent(this IElement el)
    {
        return el.TextContent.Trim();
    }

    public static IEnumerable<string> ChildTrimmedTextContent(this IElement el)
    {
        return el.Children.Select(c => c.TextContent.Trim());
    }
}