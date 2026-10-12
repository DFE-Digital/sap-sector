using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Geography;
using SAPSec.Core.Sorting;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchoolResult(
    string URN,
    string Name,
    Address Address,
    LocalAuthority LocalAuthority,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<string>> SortValue);
