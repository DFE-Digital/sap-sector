using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolSearchControllerTests
{
    private readonly Mock<ILogger<SchoolSearchController>> _mockLogger;
    private readonly Mock<ISchoolSearchService> _mockSearchService;
    private readonly SchoolSearchController _controller;

    private static Establishment FakeEstablishment1 = new()
    {
        URN = "123456",
        UKPRN = "10",
        LAId = "100",
        EstablishmentNumber = "1",
        EstablishmentName = "Fake Establishment One",
        LAName = "Leeds",
        Easting = "430000",
        Northing = "433000",
        Latitude = "53.8",
        Longitude = "-1.55"
    };

    private static Establishment FakeEstablishment2 = new()
    {
        URN = "789456",
        UKPRN = "10",
        LAId = "100",
        EstablishmentNumber = "1",
        EstablishmentName = "Fake Establishment Two",
        LAName = "Leeds",
        Easting = "430100",
        Northing = "433100",
        Latitude = "53.81",
        Longitude = "-1.54"
    };

    public SchoolSearchControllerTests()
    {
        _mockLogger = new Mock<ILogger<SchoolSearchController>>();
        _mockSearchService = new Mock<ISchoolSearchService>();
        _controller = new SchoolSearchController(_mockLogger.Object, _mockSearchService.Object);
    }

    #region Index GET Tests

    [Fact]
    public void Index_Get_ReturnsViewResult()
    {
        var result = _controller.Index();

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_Get_ReturnsViewWithEmptyViewModel()
    {
        var result = _controller.Index() as ViewResult;

        result.Should().NotBeNull();
        result.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<SchoolSearchQueryViewModel>();

        var model = result.Model as SchoolSearchQueryViewModel;
        model!.Query.Should().BeEmpty();
        model.Urn.Should().BeNull();
    }

    #endregion

    #region Index POST Tests

    [Fact]
    public async Task Index_Post_WithValidQuery_RedirectsToSearch()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School"
        };

        var result = await _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().ContainKey("query");
        redirectResult.RouteValues!["query"].Should().Be("Test School");
    }

    [Fact]
    public async Task Index_Post_WithInvalidShortQuery_ReturnsViewWithModelError()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB"
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn (minimum 3 characters)");

        var result = await _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
    }

    [Fact]
    public async Task Index_Post_WithEmptyQuery_ReturnsViewWithModelError()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = string.Empty
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn to start a search");

        var result = await _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
        _controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Index_Post_WithThreeCharQuery_RedirectsToSearch()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "ABC"
        };

        var result = await _controller.Index(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult!.RouteValues!["query"].Should().Be("ABC");
    }

    [Fact]
    public async Task Index_Post_WithQueryAndUln_RedirectsToSchoolDetails()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "School",
            Urn = "123456"
        };

        _mockSearchService.Setup(s => s.SearchByNumberAsync(viewModel.Urn))
            .ReturnsAsync(FakeEstablishment1);

        var result = await _controller.Index(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Index_Post_WithNumericResults_RedirectsToSchoolDetails()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "123/123"
        };

        var result = await _controller.Index(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["query"].Should().Be("123/123");
    }

    #endregion

    #region Search GET Tests

    [Fact]
    public async Task Search_Get_WithValidQuery_ReturnsViewWithResults()
    {
        var query = "Fake Establishment";
        var searchResults = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Fake Establishment One", FakeEstablishment1),
            SchoolSearchResult.FromNameAndEstablishment("Fake Establishment Two", FakeEstablishment2)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query, null, 1);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<SchoolSearchResultsViewModel>();

        var model = viewResult.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
        model.Results.Should().HaveCount(2);
        model.Results[0].SchoolName.Should().Be("Fake Establishment One");
        model.Results[0].URN.Should().Be("123456");
        model.Results[1].SchoolName.Should().Be("Fake Establishment Two");
        model.Results[1].URN.Should().Be("789456");
    }

    [Fact]
    public async Task Search_Get_WithNullQuery_SearchesWithEmptyString()
    {
        _mockSearchService.Setup(s => s.SearchAsync(string.Empty))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search((string?)null, null, 1);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeEmpty();
        model.Results.Should().BeEmpty();

        _mockSearchService.Verify(s => s.SearchAsync(string.Empty), Times.Once);
    }

    [Fact]
    public async Task Search_Get_WithEmptyQuery_ReturnsEmptyResults()
    {
        _mockSearchService.Setup(s => s.SearchAsync(string.Empty))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search(string.Empty, null, 1);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeEmpty();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Get_WithWhitespaceQuery_SearchesWithWhitespace()
    {
        var query = "   ";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public async Task Search_Get_WhenServiceReturnsNoResults_ReturnsEmptyResultsViewModel()
    {
        var query = "Nonexistent School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Get_WithURNQuery_ReturnsMatchingSchool()
    {
        var query = "123456";
        var searchResults = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("School by Urn", FakeEstablishment1)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query, null, 1);

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Search_Get_CallsSearchServiceWithCorrectParameters()
    {
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        await _controller.Search(query, null, 1);

        _mockSearchService.Verify(s => s.SearchAsync(query), Times.Once);
    }

    [Fact]
    public async Task Search_Get_LogsScopeWithQuery()
    {
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        await _controller.Search(query, null, 1);

        _mockLogger.Verify(x => x.BeginScope(It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task Search_Get_WithSingle_Match_RedirectsToSchoolDetails()
    {
        var query = "Saint Paul Roman Catholic";

        var returnEst = new Establishment
        {
            URN = "100273",
            UKPRN = "10",
            LAId = "100",
            EstablishmentNumber = "1",
            EstablishmentName = query
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult> { SchoolSearchResult.FromNameAndEstablishment(query, returnEst) });

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["urn"].Should().Be("100273");
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Search_Get_WithDefaultPage_ReturnsFirstPage()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.CurrentPage.Should().Be(1);
        model.Results.Should().HaveCount(10);
        model.TotalResults.Should().Be(15);
        model.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Search_Get_WithPageBeyondTotal_RedirectsToLastPage()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 10);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["page"].Should().Be(2);
    }

    [Fact]
    public async Task Search_Get_WithPageZero_TreatsAsPage1()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 0);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task Search_Get_WithNegativePage_TreatsAsPage1()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, -5);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task Search_Get_PaginationViewModel_HasCorrectStartItem()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 2);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.Pagination.StartItem.Should().Be(11);
    }

    [Fact]
    public async Task Search_Get_PaginationViewModel_HasCorrectEndItem()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 2);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.Pagination.EndItem.Should().Be(15);
    }

    [Fact]
    public async Task Search_Get_PaginationViewModel_HasPreviousPage_WhenNotOnFirstPage()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 2);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.Pagination.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Search_Get_PaginationViewModel_HasNoPreviousPage_WhenOnFirstPage()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.Pagination.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Search_Get_PaginationViewModel_HasNextPage_WhenNotOnLastPage()
    {
        var searchResults = CreateFakeSearchResults(15);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.Pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Search_Get_WithFiltersAndPagination_PreservesBothInModel()
    {
        var searchResults = CreateFakeSearchResults(15, "Leeds");
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", new[] { "Leeds" }, 2);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.CurrentPage.Should().Be(2);
        model.SelectedLocalAuthorities.Should().Contain("Leeds");
        model.Pagination.LocalAuthorities.Should().Contain("Leeds");
    }

    [Fact]
    public async Task Search_Get_WithNoResults_ReturnsZeroTotalPages()
    {
        _mockSearchService.Setup(s => s.SearchAsync("NonExistent"))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search("NonExistent", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.TotalPages.Should().Be(0);
        model.TotalResults.Should().Be(0);
    }

    [Fact]
    public async Task Search_Get_WithExactlyPageSizeResults_ReturnsSinglePage()
    {
        var searchResults = CreateFakeSearchResults(5);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.TotalPages.Should().Be(1);
        model.Results.Should().HaveCount(5);
    }

    #endregion

    #region Search POST Tests

    [Fact]
    public async Task Search_Post_WithValidQueryAndNoUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = null
        };

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().NotBeNull();
    }

    [Fact]
    public async Task Search_Post_WithUrn_RedirectsToSchoolController()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "123456"
        };
        _mockSearchService.Setup(s => s.SearchByNumberAsync(viewModel.Urn))
            .ReturnsAsync(new Establishment { URN = "123456", UKPRN = "10", LAId = "100", EstablishmentNumber = "1", EstablishmentName = "School by Urn" });

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("School");
        redirectResult.RouteValues.Should().ContainKey("urn");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Search_Post_WithWhitespaceUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "   "
        };

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.ControllerName.Should().BeNull();
    }

    [Fact]
    public async Task Search_Post_WithEmptyUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = string.Empty
        };

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
    }

    [Fact]
    public async Task Search_Post_WithInvalidShorQuery_ReturnsViewWithResultsViewModel()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB"
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn (minimum 3 characters)");

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<SchoolSearchResultsViewModel>();

        var model = viewResult.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be("AB");
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Post_WithInvalidModelStateAndNullQuery_ReturnsViewWithEmptyQuery()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = null!
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn to start a search");

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeNull();
    }

    #endregion

    #region Suggest Tests

    [Fact]
    public async Task Suggest_WithValidQueryPart_ReturnsOkWithSuggest()
    {
        var queryPart = "Test";
        var suggestions = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Test School 1", FakeEstablishment1),
            SchoolSearchResult.FromNameAndEstablishment("Test School 2", FakeEstablishment2)
        };

        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(suggestions);

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    [Fact]
    public async Task Suggest_WithDefaultTakeParameter_Uses10()
    {
        var queryPart = "Test";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(new List<SchoolSearchResult>());

        await _controller.Suggest(queryPart);

        _mockSearchService.Verify(s => s.SuggestAsync(queryPart), Times.Once);
    }

    [Fact]
    public async Task Suggest_WhenNoSuggest_ReturnsEmptyList()
    {
        var queryPart = "XYZ";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        var suggestions = okResult!.Value as List<SchoolSearchResult>;
        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task Suggest_WithEmptyQueryPart_CallsService()
    {
        var queryPart = string.Empty;
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();
        _mockSearchService.Verify(s => s.SuggestAsync(queryPart), Times.Once);
    }

    [Fact]
    public async Task Suggest_WithNullQueryPart_CallsServiceWithNull()
    {
        string? queryPart = null;
        _mockSearchService.Setup(s => s.SuggestAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Suggest(queryPart!);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Suggest_WithSingleCharacter_ReturnsResults()
    {
        var queryPart = "A";
        var suggestions = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Test School 1", FakeEstablishment1),
            SchoolSearchResult.FromNameAndEstablishment("Test School 2", FakeEstablishment2)
        };
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(suggestions);

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    [Fact]
    public async Task Suggest_WithSpecialCharacters_CallsService()
    {
        var queryPart = "St. * + Mary's";
        var suggestions = new List<SchoolSearchResult> { SchoolSearchResult.FromNameAndEstablishment("St. Mary's School", FakeEstablishment1) };
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(suggestions);

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    #endregion

    #region Edge Cases and Failure Scenarios

    [Fact]
    public async Task Search_Get_WhenServiceThrowsException_PropagatesException()
    {
        var query = "Test";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Search(query, null, 1));
    }

    [Fact]
    public async Task Suggest_WhenServiceThrowsException_PropagatesException()
    {
        var queryPart = "Test";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Suggest(queryPart));
    }

    [Fact]
    public async Task Search_Get_WithVeryLongQuery_CallsService()
    {
        var query = new string('A', 1000);
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<ViewResult>();
        _mockSearchService.Verify(s => s.SearchAsync(query), Times.Once);
    }

    [Fact]
    public async Task Search_Post_WithBothUrnAndQuery_PrioritizesUrn()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "123456"
        };

        _mockSearchService.Setup(s => s.SearchByNumberAsync(viewModel.Urn))
            .ReturnsAsync(new Establishment { URN = "123456", UKPRN = "10", LAId = "100", EstablishmentNumber = "1", EstablishmentName = "School by Urn" });

        var result = await _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Search_Get_WithMultipleSpacesInQuery_PreservesSpaces()
    {
        var query = "Test     School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public async Task Search_Get_WithSingleResult_ReturnsCorrectly()
    {
        var query = "Unique School";
        var searchResults = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Unique School", new Establishment{ URN = "999999", UKPRN = "10", LAId = "100", EstablishmentNumber = "1", EstablishmentName = "Unique School" })
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query, null, 1);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("999999");
    }

    #endregion

    #region AllResults (Map View) Tests

    [Fact]
    public async Task Search_Get_AllResults_ContainsAllSearchResults()
    {
        var searchResults = CreateFakeSearchResults(25);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults.Should().HaveCount(25, "AllResults should contain all search results for map view");
        model.Results.Should().HaveCount(10, "Results should contain only paginated results");
    }

    [Fact]
    public async Task Search_Get_AllResults_ContainsCorrectData()
    {
        var searchResults = CreateFakeSearchResults(3);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults.Should().HaveCount(3);
        model.AllResults[0].SchoolName.Should().Be("School 1");
        model.AllResults[0].URN.Should().Be("1");
        model.AllResults[1].SchoolName.Should().Be("School 2");
        model.AllResults[2].SchoolName.Should().Be("School 3");
    }

    [Fact]
    public async Task Search_Get_AllResults_RespectsLocalAuthorityFilter()
    {
        var leedsResults = CreateFakeSearchResults(5, "Leeds");
        var manchesterResults = CreateFakeSearchResults(3, "Manchester");
        var allResults = leedsResults.Concat(manchesterResults).ToList();

        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(allResults);

        var result = await _controller.Search("School", new[] { "Leeds" }, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults.Should().HaveCount(5, "AllResults should only contain filtered results");
        model.AllResults.Should().OnlyContain(s => s.LocalAuthority == "Leeds");
    }

    [Fact]
    public async Task Search_Get_AllResults_OnDifferentPages_ContainsSameResults()
    {
        var searchResults = CreateFakeSearchResults(25);
        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result1 = await _controller.Search("School", null, 1);
        var result2 = await _controller.Search("School", null, 2);

        var model1 = (result1 as ViewResult)!.Model as SchoolSearchResultsViewModel;
        var model2 = (result2 as ViewResult)!.Model as SchoolSearchResultsViewModel;

        model1!.AllResults.Should().HaveCount(25);
        model2!.AllResults.Should().HaveCount(25);
        model1.AllResults.Should().BeEquivalentTo(model2.AllResults, "AllResults should be same across all pages");
    }

    [Fact]
    public async Task Search_Get_AllResults_IsEmpty_WhenNoResults()
    {
        _mockSearchService.Setup(s => s.SearchAsync("NonExistent"))
            .ReturnsAsync(new List<SchoolSearchResult>());

        var result = await _controller.Search("NonExistent", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults.Should().BeEmpty();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Get_AllResults_ContainsLatitudeAndLongitude()
    {
        var establishment1 = new Establishment
        {
            URN = "123",
            UKPRN = "10",
            LAId = "100",
            EstablishmentNumber = "1",
            EstablishmentName = "Test School",
            LAName = "Leeds",
            Easting = "430000",
            Northing = "433000",
            Latitude = "53.8008",
            Longitude = "-1.5491"
        };
        var establishment2 = new Establishment
        {
            URN = "456",
            UKPRN = "10",
            LAId = "100",
            EstablishmentNumber = "2",
            EstablishmentName = "Another School",
            LAName = "Leeds",
            Easting = "430100",
            Northing = "433100"
        };
        var searchResults = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Test School", establishment1),
            SchoolSearchResult.FromNameAndEstablishment("Another School", establishment2)
        };

        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults[0].Latitude.Should().Be("53.8008");
        model.AllResults[0].Longitude.Should().Be("-1.5491");
    }

    [Fact]
    public async Task Search_Get_AllResults_ContainsFormattedAddress()
    {
        var establishment1 = new Establishment
        {
            URN = "123",
            UKPRN = "10",
            LAId = "100",
            EstablishmentNumber = "1",
            EstablishmentName = "Test School",
            LAName = "Leeds",
            Easting = "430000",
            Northing = "433000",
            Street = "123 Main St",
            Locality = "City Center",
            Postcode = "LS1 1AA"
        };
        var establishment2 = new Establishment
        {
            URN = "456",
            UKPRN = "10",
            LAId = "100",
            EstablishmentNumber = "2",
            EstablishmentName = "Another School",
            LAName = "Leeds",
            Easting = "430100",
            Northing = "433100"
        };
        var searchResults = new List<SchoolSearchResult>
        {
            SchoolSearchResult.FromNameAndEstablishment("Test School", establishment1),
            SchoolSearchResult.FromNameAndEstablishment("Another School", establishment2)
        };

        _mockSearchService.Setup(s => s.SearchAsync("School"))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search("School", null, 1);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;

        model!.AllResults[0].Address.Should().Be("123 Main St, City Center, Leeds, LS1 1AA");
    }

    #endregion

    #region Helper Methods

    private List<SchoolSearchResult> CreateFakeSearchResults(int count, string localAuthority = "Leeds")
    {
        var results = new List<SchoolSearchResult>();
        for (int i = 1; i <= count; i++)
        {
            var establishment = new Establishment
            {
                URN = i.ToString(),
                UKPRN = "10",
                LAId = "100",
                EstablishmentNumber = i.ToString(),
                EstablishmentName = $"School {i}",
                LAName = localAuthority,
                Easting = (430000 + i).ToString(),
                Northing = (433000 + i).ToString(),
                Latitude = (53.8 + (i * 0.01)).ToString(),
                Longitude = (-1.55 + (i * 0.01)).ToString()
            };
            results.Add(SchoolSearchResult.FromNameAndEstablishment($"School {i}", establishment));
        }
        return results;
    }

    #endregion
}