using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using System.Net;
using System.Xml.Linq;

namespace SAPSec.Test.Common.AngleSharp;

public static class DocumentExtensions
{
    public static IHtmlElement ElementWithTestIdShouldExist(this IDocument doc, string testId)
        => doc.ElementWithTestIdShouldExist<IHtmlElement>(testId);

    public static T ElementWithTestIdShouldExist<T>(this IDocument doc, string testId)
        where T : IHtmlElement
        => doc.ElementShouldExist<T>($"[data-testid=\"{testId}\"]");

    public static IHtmlElement ElementShouldExist(this IDocument doc, string selector)
        => doc.ElementShouldExist<IHtmlElement>(selector);

    public static T ElementShouldExist<T>(this IDocument doc, string selector)
        where T : IHtmlElement
    {
        var el = doc.QuerySelector(selector);
        el.Should().NotBeNull();
        return el.Should().BeAssignableTo<T>().Subject;
    }

    public static async Task<IDocument> SubmitContainingFormAsync(this IDocument doc, IHtmlButtonElement button, params HttpStatusCode[] expectedStatusCodes)
    {
        if (!expectedStatusCodes.Any())
        {
            expectedStatusCodes = [HttpStatusCode.OK];
        }

        var form = button.Ancestors<IHtmlFormElement>().FirstOrDefault();
        form.Should().NotBeNull();

        var document = await form.SubmitAsync(button);
        document.StatusCode.Should().BeOneOf(expectedStatusCodes);

        return document;
    }
}