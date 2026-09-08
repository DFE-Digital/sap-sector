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

    public static IReadOnlyCollection<IHtmlElement> ElementsShouldExist(this IDocument doc, string selector)
        => doc.ElementsShouldExist<IHtmlElement>(selector);

    public static IReadOnlyCollection<T> ElementsShouldExist<T>(this IDocument doc, string selector)
        where T : IHtmlElement
    {
        var els = doc.QuerySelectorAll(selector);
        els.Should().NotBeEmpty();

        return els.Should().AllBeAssignableTo<T>().Subject.ToList();
    }

    public static IHtmlElement ElementWithTextContentShouldExist(this IDocument doc, string selector, string textContent)
        => doc.ElementWithTextContentShouldExist<IHtmlElement>(selector, textContent);

    public static T ElementWithTextContentShouldExist<T>(this IDocument doc, string selector, string textContent)
        where T : IHtmlElement
    {
        var els = doc.QuerySelectorAll(selector);
        var el = els.FirstOrDefault(e => e.TrimmedTextContent() == textContent);
        el.Should().NotBeNull();
        return el.Should().BeAssignableTo<T>().Subject;
    }

    public static IHtmlElement DefinitionShouldExistForTerm(this IDocument doc, string termText)
    {
        var terms = doc.QuerySelectorAll("dt");
        var term = terms.FirstOrDefault(t => t.TrimmedTextContent() == termText);
        term.Should().NotBeNull();

        var index = term.ParentElement?.Children.Index(term) ?? -1;

        index.Should().BeGreaterThanOrEqualTo(0);

        var definition = term.ParentElement?.QuerySelector($":scope > dd:nth-of-type({index + 1})");
        definition.Should().NotBeNull();
        return definition.Should().BeAssignableTo<IHtmlElement>().Subject;
    }

    public static IHtmlElement TermShouldExistWithDefinition(this IDocument doc, string termText, string definitionText)
    {
        var definition = doc.DefinitionShouldExistForTerm(termText);
        definition.TrimmedTextContent().Should().Be(definitionText);

        return definition;
    }

    public static IHtmlElement TermShouldExistWithDefinition(this IDocument doc, string termText, Action<IHtmlElement> definitionAssertion)
    {
        var definition = doc.DefinitionShouldExistForTerm(termText);
        definitionAssertion(definition);

        return definition;
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