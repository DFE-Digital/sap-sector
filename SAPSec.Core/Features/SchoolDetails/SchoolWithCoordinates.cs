using SAPSec.Core.Geography;

namespace SAPSec.Core.Features.SchoolDetails;

public record SchoolWithCoordinates(SchoolInfo.School School, GeographicCoordinates? Coordinates);
