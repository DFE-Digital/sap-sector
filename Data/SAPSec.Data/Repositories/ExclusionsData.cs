using SAPSec.Data.Dto.Exclusions;

namespace SAPSec.Data.Repositories;

public record ExclusionsData(
    string Urn,
    EstablishmentExclusions? EstablishmentExclusions) : IMeasureData;
