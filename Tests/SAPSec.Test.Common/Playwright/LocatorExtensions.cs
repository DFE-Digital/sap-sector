using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class LocatorExtensions
{
    public static async Task<string?> TrimmedTextContentAsync(this ILocator locator)
    {
        var text = await locator.TextContentAsync();

        return text?.Trim();
    }
}
