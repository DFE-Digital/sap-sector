using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class SchoolTests
{
    [Fact]
    public void IsClosed_ReturnsTrue_ForClosedStatusId()
    {
        var school = School.Create(new Establishment
        {
            EstablishmentStatusId = EstablishmentStatusId.ClosedId
        });

        school.IsClosed.Should().BeTrue();
    }

    //[Fact]
    //public void IsClosed_ReturnsTrue_ForClosedStatusNameWhenIdMissing()
    //{
    //    EstablishmentStatusId.IsClosed(null, EstablishmentStatusId.Closed).Should().BeTrue();
    //}

    [Theory]
    [InlineData(EstablishmentStatusId.OpenId)]
    [InlineData(EstablishmentStatusId.OpenButProposedToCloseId)]
    [InlineData(EstablishmentStatusId.ProposedToOpenId)]
    public void IsClosed_ReturnsFalse_ForNonClosedStatusIds(string statusId)
    {
        var school = School.Create(new Establishment
        {
            EstablishmentStatusId = statusId
        });

        school.IsClosed.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsClosed_ReturnsFalse_WhenStatusIsUnknown(string? statusId)
    {
        var school = School.Create(new Establishment
        {
            EstablishmentStatusId = statusId
        });

        school.IsClosed.Should().BeFalse();
    }
}
