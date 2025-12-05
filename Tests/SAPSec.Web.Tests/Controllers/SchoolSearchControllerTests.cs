using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using SAPSec.Infrastructure.Entities;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolSearchControllerTests
{
    private readonly Mock<ILogger<SchoolSearchController>> _mockLogger;
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly SchoolSearchController _controller;

    private static Establishment FakeEstablishment1 = new()
    {
        URN = "123456",
        UKPRN = 10,
        LAId = 100,
        EstablishmentNumber = 1,
        EstablishmentName = "Fake Establishment One"
    };

    private static Establishment FakeEstablishment2 = new()
    {
        URN = "789456",
        UKPRN = 10,
        LAId = 100,
        EstablishmentNumber = 1,
        EstablishmentName = "Fake Establishment Two"
    };

    public SchoolSearchControllerTests()
    {
        _mockLogger = new Mock<ILogger<SchoolSearchController>>();
        _mockSearchService = new Mock<ISearchService>();
        _controller = new SchoolSearchController(_mockLogger.Object, _mockSearchService.Object);
    }

    #region Index GET Tests

    [Fact]
    public void Index_Get_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_Get_ReturnsViewWithEmptyViewModel()
    {
        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
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
    public void Index_Post_WithValidQuery_RedirectsToSearch()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School"
        };

        var result = _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().ContainKey("query");
        redirectResult.RouteValues!["query"].Should().Be("Test School");
    }

    [Fact]
    public void Index_Post_WithInvalidShortQuery_ReturnsViewWithModelError()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB" // Too short - minimum is 3 characters
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn (minimum 3 characters)");

        var result = _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
    }

    [Fact]
    public void Index_Post_WithEmptyQuery_ReturnsViewWithModelError()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = string.Empty
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn to start a search");

        var result = _controller.Index(viewModel);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
        _controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Index_Post_WithThreeCharQuery_RedirectsToSearch()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "ABC"
        };

        var result = _controller.Index(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["query"].Should().Be("ABC");
    }

    [Fact]
    public void Index_Post_WithQueryAndUln_RedirectsToSchoolDetails()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "School",
            Urn = "123456"
        };

        _mockSearchService.Setup(s => s.SearchByNumber(viewModel.Urn))
            .Returns(FakeEstablishment1);

        var result = _controller.Index(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public void Index_Post_WithNumericSearch_RedirectsToSchoolDetails()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "123/123"
        };

        var result = _controller.Index(viewModel);

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
        var searchResults = new List<EstablishmentSearchResult>
        {
            new EstablishmentSearchResult("Fake Establishment One", FakeEstablishment1),
            new EstablishmentSearchResult("Fake Establishment Two", FakeEstablishment2)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query);

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search((string?)null);

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search(string.Empty);

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search(query);

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search(query);

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
        var searchResults = new List<EstablishmentSearchResult>
        {
            new EstablishmentSearchResult("School by Urn", FakeEstablishment1)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query);

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Search_Get_CallsSearchServiceWithCorrectParameters()
    {
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        await _controller.Search(query);

        _mockSearchService.Verify(s => s.SearchAsync(query), Times.Once);
    }

    [Fact]
    public async Task Search_Get_LogsScopeWithQuery()
    {
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        await _controller.Search(query);

        _mockLogger.Verify(x => x.BeginScope(It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task Search_Get_WithSingle_Match_RedirectsToSchoolDetails()
    {
        var query = "Saint Paul Roman Catholic";

        var returnEst = new Establishment
        {
            URN = "100273",
            UKPRN = 10,
            LAId = 100,
            EstablishmentNumber = 1,
            EstablishmentName = query
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(new List<EstablishmentSearchResult> { new EstablishmentSearchResult(query, returnEst) });

        var result = await _controller.Search(query);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["urn"].Should().Be("100273");
    }

    #endregion

    #region Search POST Tests

    [Fact]
    public void Search_Post_WithValidQueryAndNoUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = null
        };

        var result = _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().NotBeNull();
    }

    [Fact]
    public void Search_Post_WithUrn_RedirectsToSchoolController()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "123456"
        };
        _mockSearchService.Setup(s => s.SearchByNumber(viewModel.Urn))
            .Returns(new Establishment { URN = "123456", UKPRN = 10, LAId = 100, EstablishmentNumber = 1, EstablishmentName = "School by Urn" });

        var result = _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("School");
        redirectResult.RouteValues.Should().ContainKey("urn");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public void Search_Post_WithWhitespaceUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "   "
        };

        var result = _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.ControllerName.Should().BeNull();
    }

    [Fact]
    public void Search_Post_WithEmptyUrn_RedirectsToSearchGet()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = string.Empty
        };

        var result = _controller.Search(viewModel);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
    }

    [Fact]
    public void Search_Post_WithInvalidShorQuery_ReturnsViewWithResultsViewModel()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB"
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn (minimum 3 characters)");

        var result = _controller.Search(viewModel);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<SchoolSearchResultsViewModel>();

        var model = viewResult.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be("AB");
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public void Search_Post_WithInvalidModelStateAndNullQuery_ReturnsViewWithEmptyQuery()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = null!
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or Urn to start a search");

        var result = _controller.Search(viewModel);

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
        var suggestions = new List<EstablishmentSearchResult>
        {
            new EstablishmentSearchResult("Test School 1", FakeEstablishment1),
            new EstablishmentSearchResult("Test School 2", FakeEstablishment2)
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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        await _controller.Suggest(queryPart);

        _mockSearchService.Verify(s => s.SuggestAsync(queryPart), Times.Once);
    }

    [Fact]
    public async Task Suggest_WhenNoSuggest_ReturnsEmptyList()
    {
        var queryPart = "XYZ";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart))
            .ReturnsAsync(new List<EstablishmentSearchResult>());

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Suggest(queryPart);

        result.Should().BeOfType<OkObjectResult>();
        _mockSearchService.Verify(s => s.SuggestAsync(queryPart), Times.Once);
    }

    [Fact]
    public async Task Suggest_WithNullQueryPart_CallsServiceWithNull()
    {
        string? queryPart = null;
        _mockSearchService.Setup(s => s.SuggestAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Suggest(queryPart!);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Suggest_WithSingleCharacter_ReturnsResults()
    {
        var queryPart = "A";
        var suggestions = new List<EstablishmentSearchResult>
        {
            new EstablishmentSearchResult("Test School 1", FakeEstablishment1),
            new EstablishmentSearchResult("Test School 2", FakeEstablishment2)
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
        var suggestions = new List<EstablishmentSearchResult> { new EstablishmentSearchResult("St. Mary's School", FakeEstablishment1) };
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Search(query));
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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search(query);

        result.Should().BeOfType<ViewResult>();
        _mockSearchService.Verify(s => s.SearchAsync(query), Times.Once);
    }

    [Fact]
    public void Search_Post_WithBothUrnAndQuery_PrioritizesUrn()
    {
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            Urn = "123456"
        };

        _mockSearchService.Setup(s => s.SearchByNumber(viewModel.Urn))
            .Returns(new Establishment { URN = "123456", UKPRN = 10, LAId = 100, EstablishmentNumber = 1, EstablishmentName = "School by Urn" });

        var result = _controller.Search(viewModel);

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
            .ReturnsAsync(new List<EstablishmentSearchResult>());

        var result = await _controller.Search(query);

        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public async Task Search_Get_WithSingleResult_ReturnsCorrectly()
    {
        var query = "Unique School";
        var searchResults = new List<EstablishmentSearchResult>
        {
            new EstablishmentSearchResult("Unique School", new Establishment{ URN = "999999", UKPRN = 10, LAId = 100, EstablishmentNumber = 1, EstablishmentName = "Unique School" })
        };

        _mockSearchService.Setup(s => s.SearchAsync(query))
            .ReturnsAsync(searchResults);

        var result = await _controller.Search(query);

        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("999999");
    }

    #endregion
}