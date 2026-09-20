using SAPSec.Core.Features.Geography;

namespace SAPSec.Core.Features.SchoolDetails;

public record SchoolWithCoordinates(SchoolInfo.SchoolInfo School, GeographicCoordinates? Coordinates);
