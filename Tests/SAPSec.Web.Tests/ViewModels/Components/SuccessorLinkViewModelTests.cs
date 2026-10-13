using FluentAssertions;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.Tests.Deprecated.ViewModels.Components;

public class SuccessorLinkViewModelTests
{
    [Fact]
    public void FromSuccessors_SecondarySchool_BuildsSecondaryOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new School("123456", "Academy A", new("123", "Test LA"), new("1 Test Street"), PhaseOfEducationId.SecondaryId, EstablishmentStatusId.OpenId)]);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Academy A");
        result[0].Url.Should().Be("/school/secondary/123456");
    }

    [Fact]
    public void FromSuccessors_PrimarySchool_BuildsPrimaryOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new School("123456", "Academy A", new("123", "Test LA"), new("1 Test Street"), PhaseOfEducationId.PrimaryId, EstablishmentStatusId.OpenId)]);

        result[0].Url.Should().Be("/school/primary/123456");
    }

    [Fact]
    public void FromSuccessors_AllThroughSchool_BuildsAllThroughOverviewUrl()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([new School("123456", "Academy A", new("123", "Test LA"), new("1 Test Street"), PhaseOfEducationId.AllThroughId, EstablishmentStatusId.OpenId)]);

        result[0].Url.Should().Be("/school/all-through/123456");
    }

    [Fact]
    public void FromSuccessors_PreservesOrderAndCount()
    {
        var result = SuccessorLinkViewModel.FromSuccessors([
            new School("111", "First", new("123", "Test LA"), new("1 Test Street"), PhaseOfEducationId.SecondaryId, EstablishmentStatusId.OpenId),
            new School("222", "Second", new("123", "Test LA"), new("1 Test Street"), PhaseOfEducationId.SecondaryId, EstablishmentStatusId.OpenId)
        ]);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
    }
}
