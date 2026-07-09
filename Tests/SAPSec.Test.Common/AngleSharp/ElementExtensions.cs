using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;

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

    public static void ShouldHaveRows(this IHtmlTableElement table, params string[][] expectedRows)
    {
        var rows = table.QuerySelectorAll("tr").Select(r => r.QuerySelectorAll("th, td").Select(TrimmedTextContent));
        rows.Should().BeEquivalentTo(expectedRows);
    }

    public static void ShouldLinkTo(this IElement el, string text, string path)
    {
        el.TrimmedTextContent().Should().Be(text);
        el.GetAttribute("href").Should().Be(path);
    }
}