using Microsoft.Playwright;

namespace SAPSec.Test.Common.Playwright;

public static class PageExtensions
{
    public static async Task<ILocator> GetByDescription(this IPage page, string description)
    {
        var describedElements = page.Locator("[aria-describedby]");
        var descriptionIds = await describedElements.EvaluateAllAsync<string[]>("els => els.map(el => el.getAttribute('aria-describedby'))");
        var descriptions = page.Locator(string.Join(",", descriptionIds.Select(id => $"#{id}")));
        var matchingDescription = descriptions.Filter(new() { HasText = description }).First;
        var matchingDescriptionId = await matchingDescription.GetAttributeAsync("id");
        var matchingElement = page.Locator($"[aria-describedby='{matchingDescriptionId}']");

        return matchingElement;
    }
}