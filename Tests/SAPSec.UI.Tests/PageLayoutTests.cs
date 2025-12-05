using FluentAssertions;
using SAPSec.UI.Tests.Infrastructure;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class PageLayoutTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string HomePagePath = "/";

    #region Page Structure & Metadata Tests

    [Fact]
    public async Task Layout_HasCorrectDoctypeAndHtmlAttributes()
    {
        await Page.GotoAsync(HomePagePath);

        // Validate DOCTYPE
        var html = await Page.EvaluateAsync<string>("document.doctype.name");
        html.Should().Be("html", "Document should have <!DOCTYPE html>");

        // Validate lang="en"
        var htmlLang = await Page.EvaluateAsync<string>("document.documentElement.lang");
        htmlLang.Should().Be("en", "HTML element should have lang='en'");

        // Validate viewport meta
        var viewportMeta = await Page.Locator("meta[name='viewport']").GetAttributeAsync("content");
        viewportMeta.Should().Be("width=device-width, initial-scale=1, viewport-fit=cover");
    }

    [Fact]
    public async Task Layout_HasCorrectTitleWithServiceName()
    {
        await Page.GotoAsync(HomePagePath);

        var title = await Page.TitleAsync();
        title.Should().Be($"{PageTitles.ServiceHome} - {LayoutConstants.ServiceName} - GOV.UK", "Page title should include ServiceName and GOV.UK suffix");
    }

    [Fact]
    public async Task Layout_HasCorrectFaviconsAndManifest()
    {
        await Page.GotoAsync(HomePagePath);

        // Check favicon.ico
        var faviconIco = await Page.Locator("link[rel='icon'][sizes='48x48']").GetAttributeAsync("href");
        faviconIco.Should().Be("/assets/images/favicon.ico");

        // Check favicon.svg
        var href = await Page.Locator("link[rel='icon'][href$='.svg']").GetAttributeAsync("href");
        var type = await Page.Locator("link[rel='icon'][href$='.svg']").GetAttributeAsync("type");
        href.Should().Be("/assets/images/favicon.svg");
        type.Should().Be("image/svg+xml");

        // Check mask-icon
        var maskIconHref =  await Page.Locator("link[rel='mask-icon']").GetAttributeAsync("href");
        var maskIconColor = await Page.Locator("link[rel='mask-icon']").GetAttributeAsync("color");
        maskIconHref.Should().Be("/assets/images/govuk-icon-mask.svg");
        maskIconColor.Should().Be("#0b0c0c");

        // Check apple-touch-icon
        var appleIcon = await Page.Locator("link[rel='apple-touch-icon']").GetAttributeAsync("href");
        appleIcon.Should().Be("/assets/images/govuk-icon-180.png");

        // Check manifest
        var manifest = await Page.Locator("link[rel='manifest']").GetAttributeAsync("href");
        manifest.Should().Be("/assets/manifest.json");
    }

    #endregion

    #region Header Tests

    [Fact]
    public async Task Layout_HasHeaderWithDepartmentForEducationLogo()
    {
        await Page.GotoAsync(HomePagePath);

        var logo = Page.Locator("img.govuk-header__logotype");
        var altText = await logo.GetAttributeAsync("alt");
        var src = await logo.GetAttributeAsync("src");

        (await logo.IsVisibleAsync()).Should().BeTrue();
        altText.Should().Be("Department for Education");
        src.Should().Be("/assets/images/department-for-education_white.png");
    }

    [Fact]
    public async Task Layout_HasServiceNameLinkInHeader()
    {
        await Page.GotoAsync(HomePagePath);

        var serviceNameLink = Page.Locator(".govuk-service-navigation__link").First;
        var linkText = await serviceNameLink.TextContentAsync();
        var href = await serviceNameLink.GetAttributeAsync("href");

        (await serviceNameLink.IsVisibleAsync()).Should().BeTrue();
        linkText.Should().Be(LayoutConstants.ServiceName);
        href.Should().Be("/");
    }

    #endregion

    #region Footer Tests

    [Fact]
    public async Task Layout_HasFooterWithCorrectLinks()
    {
        await Page.GotoAsync(HomePagePath);

        var footerLinks = new Dictionary<string, string>
        {
            { "Cookies", "/StaticContent/Cookies" },
            { "Accessibility", "/accessibility" },
            { "Terms of use", "/terms-of-use" },
            { "Privacy", "https://www.gov.uk/government/publications/privacy-information-education-providers-workforce-including-teachers/privacy-information-education-providers-workforce-including-teachers" }
        };

        foreach (var (linkText, expectedHref) in footerLinks)
        {
            var link = Page.Locator($"a.govuk-footer__link:has-text(\"{linkText}\")");
            (await link.IsVisibleAsync()).Should().BeTrue();
            (await link.GetAttributeAsync("href")).Should().Be(expectedHref);
        }
    }

    [Fact]
    public async Task Layout_HasCrownCopyrightAndOpenGovernmentLicence()
    {
        await Page.GotoAsync(HomePagePath);

        // Crown copyright link
        var crownLink = Page.Locator("a.govuk-footer__copyright-logo");
        (await crownLink.IsVisibleAsync()).Should().BeTrue();
        (await crownLink.GetAttributeAsync("href")).Should().Be("https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/");

        // Open Government Licence link
        var oglLink = Page.Locator("a.govuk-footer__link[href='https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/']");
        (await oglLink.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Layout_HasLicenceLogoAndText()
    {
        await Page.GotoAsync(HomePagePath);

        var licenceLogo = Page.Locator("svg.govuk-footer__licence-logo");
        var licenceText = Page.Locator("span.govuk-footer__licence-description");

        (await licenceLogo.IsVisibleAsync()).Should().BeTrue();
        (await licenceText.InnerTextAsync()).Should().Contain("All content is available under the Open Government Licence v3.0");
    }

    #endregion

    #region Skip Link & JavaScript

    [Fact]
    public async Task Layout_HasSkipToMainLink()
    {
        await Page.GotoAsync(HomePagePath);

        var skipLink = Page.Locator("a.govuk-skip-link");
        (await skipLink.IsVisibleAsync()).Should().BeTrue();
        (await skipLink.GetAttributeAsync("href")).Should().Be("#main-content");
    }

    [Fact]
    public async Task Layout_HasJSenabledClassAddedToBody()
    {
        await Page.GotoAsync(HomePagePath);

        var bodyClass = await Page.EvaluateAsync<string>("document.body.className");
        bodyClass.Should().Contain("js-enabled", "Body should have 'js-enabled' class added by script");
    }

    #endregion

    #region Assets & Scripts

    [Fact]
    public async Task Layout_LoadsRequiredCSSAndJS()
    {
        await Page.GotoAsync(HomePagePath);

        var cssFiles = new[] {
            "/css/accessible-autocomplete.min.css",
            "/css/dfefrontend.css",
            "/css/main.css"
        };

        foreach (var css in cssFiles)
        {
            var link = Page.Locator($"link[href='{css}']");
            link.Should().NotBeNull();
        }

        var jsFiles = new[] {
            "/js/dfefrontend.js",
            "/js/govuk-frontend.min.js"
        };

        foreach (var js in jsFiles)
        {
            var script = Page.Locator($"script[type='module'][src='{js}']");
            script.Should().NotBeNull();
        }
    }

    #endregion
}
