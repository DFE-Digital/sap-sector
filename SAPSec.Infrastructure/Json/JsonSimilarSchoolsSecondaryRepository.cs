using SAPSec.Core.Model.Generated;
using SAPSec.Data;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly IJsonFile<SimilarSchoolsSecondaryGroupsEntry> _similarSchoolsGroupsRepository;
    private readonly IJsonFile<SimilarSchoolsSecondaryValuesEntry> _similarSchoolsValuesRepository;
    private readonly IJsonFile<Establishment> _establishmentRepository;
    private readonly IJsonFile<EstablishmentPerformance> _establishmentPerformanceRepository;
    private readonly IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> _standardDeviationsRepository;

    public JsonSimilarSchoolsSecondaryRepository(
        IJsonFile<SimilarSchoolsSecondaryGroupsEntry> similarSchoolsGroupsRepository,
        IJsonFile<SimilarSchoolsSecondaryValuesEntry> similarSchoolsValuesRepository,
        IJsonFile<Establishment> establishmentRepository,
        IJsonFile<EstablishmentPerformance> establishmentPerformanceRepository,
        IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> standardDeviationsRepository)
    {
        _similarSchoolsGroupsRepository = similarSchoolsGroupsRepository;
        _similarSchoolsValuesRepository = similarSchoolsValuesRepository;
        _establishmentRepository = establishmentRepository;
        _establishmentPerformanceRepository = establishmentPerformanceRepository;
        _standardDeviationsRepository = standardDeviationsRepository;
    }

    //public async Task<IReadOnlyCollection<string>> GetSimilarSchoolUrnsAsync(string urn)
    //{
    //    var rows = await _similarSchoolsGroupsRepository.ReadAllAsync();
    //    var groupRows = rows.Where(r => r.URN == urn).ToList();
    //    var neighbourUrns = groupRows.Select(r => r.NeighbourURN);

    //    return neighbourUrns.ToList().AsReadOnly();
    //}

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        var allEstabs = await _establishmentRepository.ReadAllAsync();
        var currentEstab = allEstabs.Single(e => e.URN == urn);

        var rows = await _similarSchoolsGroupsRepository.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        return groupRows.AsReadOnly();

        //var neighbourUrns = groupRows.Select(r => r.NeighbourURN).ToList();

        //var similarSchoolsEstabs = allEstabs
        //    .Where(p => neighbourUrns.Contains(p.URN))
        //    .ToList();

        //var allPerformance = await _establishmentPerformanceRepository.ReadAllAsync();
        //var currentSchoolPerformance = allPerformance.Single(p => p.Id == urn);
        //var similarSchoolsPerformance = allPerformance
        //    .Where(p => neighbourUrns.Contains(p.Id))
        //    .ToList();

        //return (
        //    //FromJson(currentEstab, [currentSchoolPerformance]),
        //    similarSchoolsEstabs
        //    .GroupJoin(similarSchoolsPerformance, d => d.URN, a => a.Id, FromJson)
        //    .ToList()
        //    .AsReadOnly());
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(
        IEnumerable<string> urns)
    {
        if (urns is null)
        {
            return Array.Empty<SimilarSchoolsSecondaryValuesEntry>();
        }

        var urnList = urns as IList<string> ?? urns.ToList();
        if (urnList.Count == 0)
        {
            return Array.Empty<SimilarSchoolsSecondaryValuesEntry>();
        }

        var rows = await _similarSchoolsValuesRepository.ReadAllAsync();
        var matched = rows.Where(r => urnList.Contains(r.URN)).ToList();

        return matched
            //.Select(r => new SimilarSchoolsSecondaryValues
            //{
            //    Urn = r.URN,
            //    Ks2ReadingScore = ParseDecimal(r.KS2RP),
            //    Ks2MathsScore = ParseDecimal(r.KS2MP),
            //    PupilPremiumEligibilityPercentage = ParseDecimal(r.PPPerc),
            //    PupilsWithEalPercentage = ParseDecimal(r.PercentEAL),
            //    Polar4Quintile = ParseInt(r.Polar4QuintilePupils),
            //    PupilStabilityRate = ParseDecimal(r.PStability),
            //    AverageIdaciScore = ParseDecimal(r.IdaciPupils),
            //    PupilsWithSenSupportPercentage = ParseDecimal(r.PercentSchSupport),
            //    PupilCount = ParseInt(r.NumberOfPupils),
            //    PupilsWithEhcPlanPercentage = ParseDecimal(r.PercentageStatementOrEHP)
            //})
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviationsEntry> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
    {
        var list = await _standardDeviationsRepository.ReadAllAsync();
        return list.First();
    }
}
