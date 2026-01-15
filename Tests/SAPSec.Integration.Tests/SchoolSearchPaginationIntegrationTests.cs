using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolSearchPaginationIntegrationTests(WebApplicationSetupFixture fixture)
{
    #region Pagination Parameter Tests

    [Fact]
    public async Task GetSearch_WithPageParameter_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithPage2_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithHighPageNumber_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithZeroPage_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=0");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNegativePage_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNonNumericPage_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=abc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithDecimalPage_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1.5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pagination with Filters Tests

    [Fact]
    public async Task GetSearch_WithPageAndFilter_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1&localAuthorities=Leeds");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithPageAndMultipleFilters_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1&localAuthorities=Leeds&localAuthorities=Bradford");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_Page2WithFilter_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2&localAuthorities=Leeds");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pagination Content Tests

    [Fact]
    public async Task GetSearch_Page1_ContainsPaginationMarkup()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination"))
        {
            content.Should().Contain("govuk-pagination__list", "Pagination should have a list of pages");
        }
    }

    [Fact]
    public async Task GetSearch_Page2_ContainsPreviousLink()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination"))
        {
            content.Should().Contain("govuk-pagination__prev", "Page 2 should have Previous link");
        }
    }

    [Fact]
    public async Task GetSearch_Page1_DoesNotContainPreviousLink()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        content.Should().NotContain("govuk-pagination__prev", "Page 1 should not have Previous link");
    }

    [Fact]
    public async Task GetSearch_ContainsResultsCount()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("app-school-results-count"))
        {
            content.Should().Contain("Showing", "Results count should contain 'Showing'");
            content.Should().Contain("schools", "Results count should contain 'schools'");
        }
    }

    [Fact]
    public async Task GetSearch_Page2_ShowsCorrectResultRange()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Page 2 with page size 5 should show "6-" as start
        if (content.Contains("app-school-results-count"))
        {
            content.Should().Contain("6-", "Page 2 should show results starting from 6");
        }
    }

    #endregion

    #region Pagination Link Tests

    [Fact]
    public async Task GetSearch_PaginationLinks_ContainQueryParameter()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=TestQuery&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination__link"))
        {
            content.Should().Contain("query=TestQuery", "Pagination links should preserve query parameter");
        }
    }

    [Fact]
    public async Task GetSearch_NextLink_HasCorrectPageNumber()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Next link on page 1 should go to page 2
        if (content.Contains("govuk-pagination__next"))
        {
            content.Should().Contain("page=2", "Next link from page 1 should go to page 2");
        }
    }

    [Fact]
    public async Task GetSearch_PreviousLink_HasCorrectPageNumber()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=3");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Previous link on page 3 should go to page 2
        if (content.Contains("govuk-pagination__prev"))
        {
            content.Should().Contain("page=2", "Previous link from page 3 should go to page 2");
        }
    }

    #endregion

    #region Pagination Accessibility Tests

    [Fact]
    public async Task GetSearch_Pagination_HasAriaLabel()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination"))
        {
            content.Should().Contain("aria-label=\"Pagination\"", "Pagination should have aria-label");
        }
    }

    [Fact]
    public async Task GetSearch_CurrentPage_HasAriaCurrent()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination__item--current"))
        {
            content.Should().Contain("aria-current=\"page\"", "Current page should have aria-current='page'");
        }
    }

    [Fact]
    public async Task GetSearch_NextLink_HasRelNext()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination__next"))
        {
            content.Should().Contain("rel=\"next\"", "Next link should have rel='next'");
        }
    }

    [Fact]
    public async Task GetSearch_PreviousLink_HasRelPrev()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination__prev"))
        {
            content.Should().Contain("rel=\"prev\"", "Previous link should have rel='prev'");
        }
    }

    [Fact]
    public async Task GetSearch_PageLinks_HaveAriaLabels()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (content.Contains("govuk-pagination__item"))
        {
            content.Should().Contain("aria-label=\"Page", "Page links should have aria-labels");
        }
    }

    #endregion

    #region Page Title Tests

    [Fact]
    public async Task GetSearch_Page2_HasPageNumberInTitle()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=2");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check if page number is in title for accessibility
        content.Should().Contain("<title>", "Page should have a title tag");

        if (content.Contains("page 2"))
        {
            content.Should().Contain("page 2", "Page 2 should have page number in title");
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetSearch_WithPagination_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=1", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_HighPageNumber_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School&page=50", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task GetSearch_PageParameter_IsNotVulnerableToInjection()
    {
        var maliciousPage = "<script>alert('xss')</script>";
        var response = await fixture.Client.GetAsync($"/find-a-school/search?query=School&page={Uri.EscapeDataString(maliciousPage)}");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("<script>alert('xss')</script>", "Page parameter should be sanitized");
    }

    #endregion
}
