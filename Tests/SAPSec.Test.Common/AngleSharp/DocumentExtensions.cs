using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using System.Net;
using System.Xml.Linq;

namespace SAPSec.Test.Common.AngleSharp;

public static class DocumentExtensions
{
    public static IElement ElementShouldExist(this IDocument doc, string selector)
    {
        var element = doc.QuerySelector(selector);
        element.Should().NotBeNull();

        return element;
    }

    public static IHtmlInputElement InputWithNameShouldExist(this IDocument doc, string name)
    {
        return doc.QuerySelector($@"input[name=""{name}""]")
            .Should().NotBeNull().And.BeAssignableTo<IHtmlInputElement>()
            .Subject;
    }

    public static IHtmlButtonElement ButtonWithNameShouldExist(this IDocument doc, string name)
    {
        return doc.QuerySelector($@"button[name=""{name}""]")
            .Should().NotBeNull().And.BeAssignableTo<IHtmlButtonElement>()
            .Subject;
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
