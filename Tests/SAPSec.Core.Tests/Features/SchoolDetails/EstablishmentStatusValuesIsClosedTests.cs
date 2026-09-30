using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class EstablishmentStatusValuesIsClosedTests
{
    [Fact]
    public void IsClosed_ReturnsTrue_ForClosedStatusId()
    {
        EstablishmentStatusValues.IsClosed(EstablishmentStatusValues.ClosedId, null).Should().BeTrue();
    }

    [Fact]
    public void IsClosed_ReturnsTrue_ForClosedStatusNameWhenIdMissing()
    {
        EstablishmentStatusValues.IsClosed(null, EstablishmentStatusValues.Closed).Should().BeTrue();
    }

    [Theory]
    [InlineData(EstablishmentStatusValues.OpenId)]
    [InlineData(EstablishmentStatusValues.OpenButProposedToCloseId)]
    [InlineData(EstablishmentStatusValues.ProposedToOpenId)]
    public void IsClosed_ReturnsFalse_ForNonClosedStatusIds(string statusId)
    {
        EstablishmentStatusValues.IsClosed(statusId, null).Should().BeFalse();
    }

    [Fact]
    public void IsClosed_ReturnsFalse_WhenStatusIsUnknown()
    {
        EstablishmentStatusValues.IsClosed(null, null).Should().BeFalse();
        EstablishmentStatusValues.IsClosed("", "").Should().BeFalse();
    }
}
