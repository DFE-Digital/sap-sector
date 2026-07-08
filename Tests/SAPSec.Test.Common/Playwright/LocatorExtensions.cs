using FluentAssertions;
using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class LocatorExtensions
{
    public static async Task<string?> TrimmedTextContentAsync(this ILocator locator)
    {
        var text = await locator.TextContentAsync();

        return text?.Trim();
    }
    public static async Task<IReadOnlyList<string>> AllTrimmedTextContentsAsync(this ILocator locator)
    {
        var allText = await locator.AllTextContentsAsync();

        return allText.Select(t => t.Trim()).ToList();
    }

    public static async Task<ILocator> GetTableColumnAsync(this ILocator locator, string columnHeader)
    {
        var headers = await locator.Locator("thead tr th").AllTrimmedTextContentsAsync();
        headers.Should().Contain(columnHeader);

        var rows = locator.Locator("tbody tr");

        var columnIndex = headers.ToList().IndexOf(columnHeader);
        var cells = rows.Locator($"td:nth-child({columnIndex + 1}),th:nth-child({columnIndex + 1})");

        return cells;
    }
}
