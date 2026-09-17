using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Business rule: a closed school may only continue to appear in the service where it meets the
/// agreed data availability criteria (see <see cref="SchoolClosureEligibilityRule"/>), and the
/// closed school banner is only shown where the school has no successor relationship.
/// </summary>
public sealed class SchoolClosureEligibilityService(IEstablishmentRepository establishmentRepository)
    : ISchoolClosureEligibilityService
{
    public async Task<SchoolClosureEligibility> EvaluateAsync(Establishment establishment)
    {
        ArgumentNullException.ThrowIfNull(establishment);

        if (!EstablishmentStatusValues.IsClosed(establishment.EstablishmentStatusId, establishment.EstablishmentStatusName))
        {
            return SchoolClosureEligibility.NotClosed;
        }

        if (!SchoolClosureEligibilityRule.IsEligibleToAppear(establishment))
        {
            return new SchoolClosureEligibility(IsEligibleForDisplay: false, ShowClosedSchoolBanner: false);
        }

        var links = await establishmentRepository.GetEstablishmentLinksAsync(establishment.URN);
        var hasSuccessor = links.Any(link => EstablishmentLinkTypeValues.IsSuccessor(link.linktype));

        return new SchoolClosureEligibility(IsEligibleForDisplay: true, ShowClosedSchoolBanner: !hasSuccessor);
    }
}
