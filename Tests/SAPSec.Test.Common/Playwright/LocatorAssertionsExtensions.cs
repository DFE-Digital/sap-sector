using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class LocatorAssertionsExtensions
{
    private const string PercentageValuePattern = @"\d?\d%";
    private const string NumericValuePattern = @"\d?\d\.\d";

    public static async Task ToBePercentageValuesHavingCount(this ILocatorAssertions assertions, int count)
    {
        await assertions.ToHaveCountAsync(count);
        var locator = assertions.GetActualLocator();
        var values = await locator.AllTrimmedTextContentsAsync();
        values.Should().AllSatisfy(x => x.Should().MatchRegex(PercentageValuePattern));
    }

    public static async Task ToBeNumericValuesHavingCount(this ILocatorAssertions assertions, int count)
    {
        await assertions.ToHaveCountAsync(count);
        var locator = assertions.GetActualLocator();
        var values = await locator.AllTrimmedTextContentsAsync();
        values.Should().AllSatisfy(x => x.Should().MatchRegex(NumericValuePattern));
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
            Execute.Assertion.FailWith("Could not determine underlying ILocator for expectation.");
        }

        return locator!;
    }
}