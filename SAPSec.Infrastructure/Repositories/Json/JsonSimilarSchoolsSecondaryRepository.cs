using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Infrastructure.Repositories.Json;

namespace SAPSec.Infrastructure.Repositories;

public class JsonSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly JsonFile<SimilarSchoolsSecondaryGroupsRow> _similarSchoolsGroupsRepository;
    private readonly JsonFile<SimilarSchoolsSecondaryValuesRow> _similarSchoolsValuesRepository;
    private readonly JsonFile<Establishment> _establishmentRepository;
    private readonly JsonFile<EstablishmentPerformance> _establishmentPerformanceRepository;

    public JsonSimilarSchoolsSecondaryRepository(
        JsonFile<SimilarSchoolsSecondaryGroupsRow> similarSchoolsGroupsRepository,
        JsonFile<SimilarSchoolsSecondaryValuesRow> similarSchoolsValuesRepository,
        JsonFile<Establishment> establishmentRepository,
        JsonFile<EstablishmentPerformance> establishmentPerformanceRepository)
    {
        _similarSchoolsGroupsRepository = similarSchoolsGroupsRepository;
        _similarSchoolsValuesRepository = similarSchoolsValuesRepository;
        _establishmentRepository = establishmentRepository;
        _establishmentPerformanceRepository = establishmentPerformanceRepository;
    }

    public async Task<IReadOnlyCollection<string>> GetSimilarSchoolUrnsAsync(string urn)
    {
        var rows = await _similarSchoolsGroupsRepository.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        var neighbourUrns = groupRows.Select(r => r.NeighbourURN);

        return neighbourUrns.ToList().AsReadOnly();
    }

    public async Task<(SimilarSchool, IReadOnlyCollection<SimilarSchool>)> GetSimilarSchoolsGroupAsync(string urn)
    {
        var allEstabs = await _establishmentRepository.ReadAllAsync();
        var currentEstab = allEstabs.Single(e => e.URN == urn);

        var rows = await _similarSchoolsGroupsRepository.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        var neighbourUrns = groupRows.Select(r => r.NeighbourURN).ToList();

        var similarSchoolsEstabs = allEstabs
            .Where(p => neighbourUrns.Contains(p.URN))
            .ToList();

        var allPerformance = await _establishmentPerformanceRepository.ReadAllAsync();
        var currentSchoolPerformance = allPerformance.Single(p => p.Id == urn);
        var similarSchoolsPerformance = allPerformance
            .Where(p => neighbourUrns.Contains(p.Id))
            .ToList();

        return (
            FromJson(currentEstab, [currentSchoolPerformance]),
            similarSchoolsEstabs
                .GroupJoin(similarSchoolsPerformance, d => d.URN, a => a.Id, FromJson)
                .ToList()
                .AsReadOnly());
    }

    private SimilarSchool FromJson(Establishment currentEstab, IEnumerable<EstablishmentPerformance> currentSchoolPerformances)
    {
        var currentSchoolPerformance = currentSchoolPerformances.FirstOrDefault();
        return new SimilarSchool
        {
            URN = currentEstab.URN,
            Name = currentEstab.EstablishmentName,
            Address = new Address
            {
                Street = currentEstab.Street,
                Locality = currentEstab.Locality,
                Address3 = currentEstab.Address3,
                Town = currentEstab.Town,
                Postcode = currentEstab.Postcode
            },
            LocalAuthority = new Core.Features.SimilarSchools.LocalAuthority(currentEstab.LAId, currentEstab.LAName),
            Coordinates = BNGCoordinates.TryParse(currentEstab.Easting, currentEstab.Northing, out var coords) ? coords : null,
            UrbanRuralId = currentEstab.UrbanRuralId,
            UrbanRuralName = currentEstab.UrbanRuralName,
            Attainment8Score = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Attainment8_Tot_Est_Current_Num),
            BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Bio59_Sum_Est_Current_Num),
            ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Chem59_Sum_Est_Current_Num),
            CombinedSciencGcseGrade55AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.CombSci59_Sum_Est_Current_Num),
            EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngLang59_Sum_Est_Current_Num),
            EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngLit59_Sum_Est_Current_Num),
            EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngMaths59_Tot_Est_Current_Num),
            MathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Maths59_Sum_Est_Current_Num),
            PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Physics59_Sum_Est_Current_Num),
        };
    }
}