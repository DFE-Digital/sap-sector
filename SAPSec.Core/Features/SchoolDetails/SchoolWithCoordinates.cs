using SAPSec.Core.Geography;

namespace SAPSec.Core.Features.SchoolDetails;

public record SchoolWithCoordinates(SchoolInfo.SchoolInfo School, GeographicCoordinates? Coordinates);
