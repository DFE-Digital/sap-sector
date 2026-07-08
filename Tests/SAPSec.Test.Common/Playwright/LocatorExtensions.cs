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

    public static async Task ShouldBeVisibleAsync(this ILocator locator)
    {
        var isVisible = await locator.IsVisibleAsync();
        isVisible.Should().BeTrue();
    }

    public static async Task ShouldBeHiddenAsync(this ILocator locator)
    {
        var isHidden = await locator.IsHiddenAsync();
        isHidden.Should().BeTrue();
    }
}
