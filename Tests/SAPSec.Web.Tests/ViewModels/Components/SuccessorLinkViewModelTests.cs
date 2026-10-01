using FluentAssertions;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.Tests.ViewModels.Components;

public class SuccessorLinkViewModelTests
{
    [Fact]
    public void FromSuccessors_SecondarySchool_BuildsSecondaryOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new SuccessorSchool("123456", "Academy A", "Secondary")]);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Academy A");
        result[0].Url.Should().Be("/school/secondary/123456");
    }

    [Fact]
    public void FromSuccessors_PrimarySchool_BuildsPrimaryOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new SuccessorSchool("123456", "Academy A", "Primary")]);

        result[0].Url.Should().Be("/school/primary/123456");
    }

    [Fact]
    public void FromSuccessors_AllThroughSchool_BuildsAllThroughOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new SuccessorSchool("123456", "Academy A", "All-through")]);

        result[0].Url.Should().Be("/school/all-through/123456");
    }

    [Fact]
    public void FromSuccessors_PreservesOrderAndCount()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([
            new SuccessorSchool("111", "First", "Secondary"),
            new SuccessorSchool("222", "Second", "Secondary")
        ]);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
    }
}
