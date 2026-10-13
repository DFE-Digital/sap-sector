using SAPSec.Core.Authentication;
using SAPSec.Core.Features.SchoolSearch;

namespace SAPSec.Core.Features.AssociatedSchools;

public interface IAssociatedSchoolsService
{
    /// <summary>
    /// Gets the schools associated with a trust or local authority organisation.
    /// Returns no schools for any other kind of organisation.
    /// </summary>
    Task<IReadOnlyList<SchoolSearchResult>> GetAssociatedSchoolsAsync(Organisation organisation);
}
