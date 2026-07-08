using Codeuctivity.SkiaSharpCompare;
using FluentAssertions;
using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class LocatorAssertionsExtensions
{
    private const double PixelErrorPercentageThreshold = 10;

    public static async Task ToMatchScreenshotAsync(this ILocatorAssertions assertions, string screenshotName)
    {
        var locator = assertions.GetActualLocator();

        var actualPath = $@"Screenshots\{screenshotName}-actual.png";
        var expectedPath = $@"Screenshots\{screenshotName}.png";
        await locator.ScreenshotAsync(new() { Path = actualPath });
        var result = Compare.CalcDiff(actualPath, expectedPath, ResizeOption.Resize);

        result.Should().NotBeNull();
        result!.PixelErrorPercentage.Should().BeLessThan(PixelErrorPercentageThreshold);
    }

    private static ILocator GetActualLocator(this ILocatorAssertions assertions)
    {
        // BRITTLE: ActualLocator is a private property of internal LocatorAssertions class,
        // no way to get this without using reflection - this is needed to work with the
        // Expect(locator).Assertion() syntax.
        var actualLocatorProperty = assertions.GetType().GetProperty("ActualLocator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var locator = (ILocator?)actualLocatorProperty?.GetValue(assertions);

        if (locator is null)
        {
            throw new InvalidOperationException("Could not determine underlying ILocator for expectation.");
        }

        return locator;
    }
}