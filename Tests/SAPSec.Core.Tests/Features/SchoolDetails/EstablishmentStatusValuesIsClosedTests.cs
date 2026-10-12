using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class EstablishmentStatusValuesIsClosedTests
{
    [Fact]
    public void IsClosed_ReturnsTrue_ForClosedStatusId()
    {
        EstablishmentStatusId.IsClosed(EstablishmentStatusId.ClosedId, null).Should().BeTrue();
    }

    [Fact]
    public void IsClosed_ReturnsTrue_ForClosedStatusNameWhenIdMissing()
    {
        EstablishmentStatusId.IsClosed(null, EstablishmentStatusId.Closed).Should().BeTrue();
    }

    [Theory]
    [InlineData(EstablishmentStatusId.OpenId)]
    [InlineData(EstablishmentStatusId.OpenButProposedToCloseId)]
    [InlineData(EstablishmentStatusId.ProposedToOpenId)]
    public void IsClosed_ReturnsFalse_ForNonClosedStatusIds(string statusId)
    {
        EstablishmentStatusId.IsClosed(statusId, null).Should().BeFalse();
    }

    [Fact]
    public void IsClosed_ReturnsFalse_WhenStatusIsUnknown()
    {
        EstablishmentStatusId.IsClosed(null, null).Should().BeFalse();
        EstablishmentStatusId.IsClosed("", "").Should().BeFalse();
    }
}
