using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.Sorting;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public record SimilarSchoolResult(
    string URN,
    string Name,
    Address Address,
    ReferenceData LocalAuthority,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<string>> SortValue);
