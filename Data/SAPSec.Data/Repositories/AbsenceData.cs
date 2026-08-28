using SAPSec.Data.Dto.Absence;

namespace SAPSec.Data.Repositories;

public record AbsenceData(
    string Urn,
    EstablishmentAbsence? EstablishmentAbsence,
    LAAbsence? LocalAuthorityAbsence,
    EnglandAbsence? EnglandAbsence) : IMeasureData;
