using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class LocatorExtensions
{
    private const int DomPollIntervalMs = 100;
    private const int DomDebounceMs = 500;
    private const int DomTimeoutMs = 10000;

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

    public static ILocator GetCells(this ILocator locator)
    {
        var cells = locator.Locator("tbody tr th,td");

        return cells;
    }

    public static async Task WaitForDomToStopChanging(this ILocator locator, int pollIntervalMs = DomPollIntervalMs, int debounceMs = DomDebounceMs, int timeoutMs = DomTimeoutMs)
    {
        var previousHtml = "";
        var startTime = DateTime.UtcNow;
        var isStable = false;

        while (!isStable)
        {
            var currentHtml = await locator.InnerHTMLAsync();
            if (currentHtml == previousHtml)
            {
                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (timeoutMs <= elapsedMs)
                {
                    Execute.Assertion.FailWith("Timeout expired waiting for DOM to stop changing.");
                }

                isStable = debounceMs <= elapsedMs;
            }
            else
            {
                previousHtml = currentHtml;
            }

            if (!isStable)
            {
                await Task.Delay(pollIntervalMs);
            }
        }
    }
}
