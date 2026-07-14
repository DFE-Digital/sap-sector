using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using System.Net;
using System.Xml.Linq;

namespace SAPSec.Test.Common.AngleSharp;

public static class DocumentExtensions
{
    public static IElement ElementWithTestIdShouldExist(this IDocument doc, string testId)
    {
        var el = doc.QuerySelector($"[data-testid=\"{testId}\"]");
        el.Should().NotBeNull();

        return el;
    }

    public static T ElementWithTestIdShouldExist<T>(this IDocument doc, string testId)
        where T : IElement
    {
        var el = doc.QuerySelector($"[data-testid=\"{testId}\"]");
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