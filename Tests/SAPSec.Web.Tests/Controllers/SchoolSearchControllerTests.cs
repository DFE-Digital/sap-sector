using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Infrastructure.Entities;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolSearchControllerTests
{
    private readonly Mock<ILogger<SchoolSearchController>> _mockLogger;
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly SchoolSearchController _controller;

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
        result!.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<SchoolSearchQueryViewModel>();

        var model = result.Model as SchoolSearchQueryViewModel;
        model!.Query.Should().BeEmpty();
        model.EstablishmentId.Should().BeNull();
    }

    #endregion

    #region Index POST Tests

    [Fact]
    public void Index_Post_WithValidModel_RedirectsToSearch()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School"
        };

        // Act
        var result = _controller.Index(viewModel);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().ContainKey("query");
        redirectResult.RouteValues!["query"].Should().Be("Test School");
    }

    [Fact]
    public void Index_Post_WithInvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB" // Too short - minimum is 3 characters
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or URN (minimum 3 characters)");

        // Act
        var result = _controller.Index(viewModel);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
    }

    [Fact]
    public void Index_Post_WithEmptyQuery_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = string.Empty
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or URN to start a search");

        // Act
        var result = _controller.Index(viewModel);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(viewModel);
        _controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Index_Post_WithMinimumValidQuery_RedirectsToSearch()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "ABC" // Minimum 3 characters
        };

        // Act
        var result = _controller.Index(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.RouteValues!["query"].Should().Be("ABC");
    }

    #endregion

    #region Search GET Tests

    [Fact]
    public async Task Search_Get_WithValidQuery_ReturnsViewWithResults()
    {
        // Arrange
        var query = "Test School";
        var searchResults = new List<SearchResult>
        {
            new SearchResult(new Establishment(123456, "Test School 1"), 1),
            new SearchResult(new Establishment(789012, "Test School 2"), 0.9f)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<SchoolSearchResultsViewModel>();

        var model = viewResult.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
        model.Results.Should().HaveCount(2);
        model.Results[0].SchoolName.Should().Be("Test School 1");
        model.Results[0].URN.Should().Be("123456");
        model.Results[1].SchoolName.Should().Be("Test School 2");
        model.Results[1].URN.Should().Be("789012");
    }

    [Fact]
    public async Task Search_Get_WithNullQuery_SearchesWithEmptyString()
    {
        // Arrange
        _mockSearchService.Setup(s => s.SearchAsync(string.Empty, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search((string?) null);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeEmpty();
        model.Results.Should().BeEmpty();

        _mockSearchService.Verify(s => s.SearchAsync(string.Empty, 50), Times.Once);
    }

    [Fact]
    public async Task Search_Get_WithEmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        _mockSearchService.Setup(s => s.SearchAsync(string.Empty, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(string.Empty);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeEmpty();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Get_WithWhitespaceQuery_SearchesWithWhitespace()
    {
        // Arrange
        var query = "   ";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public async Task Search_Get_WhenServiceReturnsNoResults_ReturnsEmptyResultsViewModel()
    {
        // Arrange
        var query = "Nonexistent School";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Get_WithURNQuery_ReturnsMatchingSchool()
    {
        // Arrange
        var query = "123456";
        var searchResults = new List<SearchResult>
        {
            new SearchResult(new Establishment(123456, "School by URN"), 1)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Results.Should().HaveCount(1);
        model.Results[0].URN.Should().Be("123456");
    }

    [Fact]
    public async Task Search_Get_CallsSearchServiceWithCorrectParameters()
    {
        // Arrange
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        await _controller.Search(query);

        // Assert
        _mockSearchService.Verify(s => s.SearchAsync(query, 50), Times.Once);
    }

    [Fact]
    public async Task Search_Get_LogsScopeWithQuery()
    {
        // Arrange
        var query = "Test School";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        await _controller.Search(query);

        // Assert
        _mockLogger.Verify(
            x => x.BeginScope(It.IsAny<It.IsAnyType>()),
            Times.Once);
    }

    #endregion

    #region Search POST Tests

    [Fact]
    public void Search_Post_WithValidQueryAndNoEstablishmentId_RedirectsToSearchGet()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            EstablishmentId = null
        };

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.RouteValues.Should().NotBeNull();
    }

    [Fact]
    public void Search_Post_WithEstablishmentId_RedirectsToSchoolController()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            EstablishmentId = "123456"
        };

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("School");
        redirectResult.RouteValues.Should().ContainKey("urn");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public void Search_Post_WithWhitespaceEstablishmentId_RedirectsToSearchGet()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            EstablishmentId = "   "
        };

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
        redirectResult.ControllerName.Should().BeNull();
    }

    [Fact]
    public void Search_Post_WithEmptyEstablishmentId_RedirectsToSearchGet()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            EstablishmentId = string.Empty
        };

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Search");
    }

    [Fact]
    public void Search_Post_WithInvalidModelState_ReturnsViewWithResultsViewModel()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "AB" // Too short
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or URN (minimum 3 characters)");

        // Act
        var result = _controller.Search(viewModel);

        // Assert
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
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = null!
        };
        _controller.ModelState.AddModelError("Query", "Enter a school name or URN to start a search");

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().BeNull();
    }

    [Fact]
    public void Search_Post_WithMultipleErrors_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "A"
        };
        _controller.ModelState.AddModelError("Query", "Error 1");
        _controller.ModelState.AddModelError("Query", "Error 2");

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _controller.ModelState.ErrorCount.Should().Be(2);
    }

    #endregion

    #region Suggest Tests

    [Fact]
    public async Task Suggest_WithValidQueryPart_ReturnsOkWithsuggest()
    {
        // Arrange
        var queryPart = "Test";
        var suggestions = new List<string> { "Test School 1", "Test School 2", "Test Academy" };
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(suggestions);

        // Act
        var result = await _controller.Suggest(queryPart);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    [Fact]
    public async Task Suggest_WithDefaultTakeParameter_Uses10()
    {
        // Arrange
        var queryPart = "Test";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(new List<string>());

        // Act
        await _controller.Suggest(queryPart);

        // Assert
        _mockSearchService.Verify(s => s.SuggestAsync(queryPart, 10), Times.Once);
    }

    [Fact]
    public async Task Suggest_WhenNosuggest_ReturnsEmptyList()
    {
        // Arrange
        var queryPart = "XYZ";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Suggest(queryPart);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        var suggestions = okResult!.Value as List<string>;
        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task Suggest_WithEmptyQueryPart_CallsService()
    {
        // Arrange
        var queryPart = string.Empty;
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Suggest(queryPart);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockSearchService.Verify(s => s.SuggestAsync(queryPart, 10), Times.Once);
    }

    [Fact]
    public async Task Suggest_WithNullQueryPart_CallsServiceWithNull()
    {
        // Arrange
        string? queryPart = null;
        _mockSearchService.Setup(s => s.SuggestAsync(It.IsAny<string>(), 10))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Suggest(queryPart!);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Suggest_WithSingleCharacter_ReturnsResults()
    {
        // Arrange
        var queryPart = "A";
        var suggestions = new List<string> { "Academy 1", "Academy 2" };
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(suggestions);

        // Act
        var result = await _controller.Suggest(queryPart);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    [Fact]
    public async Task Suggest_WithSpecialCharacters_CallsService()
    {
        // Arrange
        var queryPart = "St. Mary's";
        var suggestions = new List<string> { "St. Mary's School" };
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ReturnsAsync(suggestions);

        // Act
        var result = await _controller.Suggest(queryPart);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(suggestions);
    }

    #endregion

    #region Edge Cases and Failure Scenarios

    [Fact]
    public async Task Search_Get_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var query = "Test";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Search(query));
    }

    [Fact]
    public async Task Suggest_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var queryPart = "Test";
        _mockSearchService.Setup(s => s.SuggestAsync(queryPart, 10))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Suggest(queryPart));
    }

    [Fact]
    public async Task Search_Get_WithVeryLongQuery_CallsService()
    {
        // Arrange
        var query = new string('A', 1000);
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _mockSearchService.Verify(s => s.SearchAsync(query, 50), Times.Once);
    }

    [Fact]
    public async Task Search_Get_WithUnicodeCharacters_CallsService()
    {
        // Arrange
        var query = "École Français 中文学校";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public void Search_Post_WithBothEstablishmentIdAndQuery_PrioritizesEstablishmentId()
    {
        // Arrange
        var viewModel = new SchoolSearchQueryViewModel
        {
            Query = "Test School",
            EstablishmentId = "123456"
        };

        // Act
        var result = _controller.Search(viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ControllerName.Should().Be("School");
        redirectResult.RouteValues!["urn"].Should().Be("123456");
    }

    [Fact]
    public async Task Search_Get_WithMultipleSpacesInQuery_PreservesSpaces()
    {
        // Arrange
        var query = "Test     School";
        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(new List<SearchResult>());

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Query.Should().Be(query);
    }

    [Fact]
    public async Task Search_Get_WithSingleResult_ReturnsCorrectly()
    {
        // Arrange
        var query = "Unique School";
        var searchResults = new List<SearchResult>
        {
            new SearchResult(new Establishment(999999, "Unique School"), 1)
        };

        _mockSearchService.Setup(s => s.SearchAsync(query, 50))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Search(query);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as SchoolSearchResultsViewModel;
        model!.Results.Should().HaveCount(1);
        model.Results[0].SchoolName.Should().Be("Unique School");
        model.Results[0].URN.Should().Be("999999");
    }

    #endregion
}
