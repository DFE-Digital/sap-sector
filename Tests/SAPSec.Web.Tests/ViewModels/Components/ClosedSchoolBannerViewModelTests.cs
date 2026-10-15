using FluentAssertions;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.Tests.ViewModels.Components;

public class ClosedSchoolBannerViewModelTests
{
    [Fact]
    public void SuccessorNamesJoined_WithNoSuccessors_ReturnsEmpty()
    {
        var model = new ClosedSchoolBannerViewModel(ClosedSchoolBannerVariant.ThisSchool, [], []);

        model.SuccessorNamesJoined.Should().BeEmpty();
        model.HasSuccessors.Should().BeFalse();
    }

    [Fact]
    public void SuccessorNamesJoined_WithOneSuccessor_ReturnsItsName()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [new SuccessorLinkViewModel("Academy A", "/school/secondary/1")],
            []);

        model.SuccessorNamesJoined.Should().Be("Academy A");
    }

    [Fact]
    public void SuccessorNamesJoined_WithTwoSuccessors_JoinsWithAnd()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [
                new SuccessorLinkViewModel("Academy A", "/school/secondary/1"),
                new SuccessorLinkViewModel("Academy B", "/school/secondary/2")
            ],
            []);

        model.SuccessorNamesJoined.Should().Be("Academy A and Academy B");
    }

    [Fact]
    public void SuccessorNamesJoined_WithThreeSuccessors_UsesCommasAndFinalAnd()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [
                new SuccessorLinkViewModel("Academy A", "/school/secondary/1"),
                new SuccessorLinkViewModel("Academy B", "/school/secondary/2"),
                new SuccessorLinkViewModel("Academy C", "/school/secondary/3")
            ],
            []);

        model.SuccessorNamesJoined.Should().Be("Academy A, Academy B and Academy C");
        model.HasSuccessors.Should().BeTrue();
    }

    [Fact]
    public void PredecessorNamesJoined_WithNoPredecessors_ReturnsEmpty()
    {
        var model = new ClosedSchoolBannerViewModel(ClosedSchoolBannerVariant.ThisSchool, [], []);

        model.PredecessorNamesJoined.Should().BeEmpty();
        model.HasPredecessors.Should().BeFalse();
    }

    [Fact]
    public void PredecessorNamesJoined_WithOnePredecessor_ReturnsItsName()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [],
            [new SuccessorLinkViewModel("Old Academy", "/school/secondary/1")]);

        model.PredecessorNamesJoined.Should().Be("Old Academy");
        model.HasPredecessors.Should().BeTrue();
    }

    [Fact]
    public void PredecessorNamesJoined_WithTwoPredecessors_JoinsWithAnd()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [],
            [
                new SuccessorLinkViewModel("Academy A", "/school/secondary/1"),
                new SuccessorLinkViewModel("Academy B", "/school/secondary/2")
            ]);

        model.PredecessorNamesJoined.Should().Be("Academy A and Academy B");
    }

    [Fact]
    public void PredecessorNamesJoined_WithThreePredecessors_UsesCommasAndFinalAnd()
    {
        var model = new ClosedSchoolBannerViewModel(
            ClosedSchoolBannerVariant.ThisSchool,
            [],
            [
                new SuccessorLinkViewModel("Academy A", "/school/secondary/1"),
                new SuccessorLinkViewModel("Academy B", "/school/secondary/2"),
                new SuccessorLinkViewModel("Academy C", "/school/secondary/3")
            ]);

        model.PredecessorNamesJoined.Should().Be("Academy A, Academy B and Academy C");
    }
}
