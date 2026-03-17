using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly ILogger<PostgresSimilarSchoolsSecondaryRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresSimilarSchoolsSecondaryRepository(ILogger<PostgresSimilarSchoolsSecondaryRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<string>> GetSimilarSchoolUrnsAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "neighbour_urn" 
            FROM public.v_similar_schools_secondary_groups 
            WHERE "urn" = @urn;
        """;

        var urns = await conn.QueryAsync<string>(sql, new { urn });

        return urns.ToList().AsReadOnly();
    }

    public async Task<(SimilarSchool?, IReadOnlyCollection<SimilarSchool>)> GetSimilarSchoolsGroupAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT
                *
            FROM public.v_establishment
            WHERE "URN" = @urn;
            
            SELECT
                *
            FROM public.v_establishment
            WHERE "URN" IN (
                SELECT "neighbour_urn" 
                FROM public.v_similar_schools_secondary_groups 
                WHERE "urn" = @urn
            );

            SELECT
                "Id",
                "Attainment8_Tot_Est_Current_Num",
                "Bio59_Sum_Est_Current_Num",
                "Chem59_Sum_Est_Current_Num",
                "CombSci59_Sum_Est_Current_Num",
                "EngLang59_Sum_Est_Current_Num",
                "EngLit59_Sum_Est_Current_Num",
                "EngMaths59_Tot_Est_Current_Num",
                "EngMaths59_Tot_Est_Current_Pct",
                "Maths59_Sum_Est_Current_Num",
                "Physics59_Sum_Est_Current_Num"
            FROM public.v_establishment_performance
            WHERE "Id" = @urn OR "Id" IN (
                SELECT "neighbour_urn" 
                FROM public.v_similar_schools_secondary_groups 
                WHERE "urn" = @urn
            );
        """;

        var results = await conn.QueryMultipleAsync(sql, new { urn });

        var currentSchool = await results.ReadSingleOrDefaultAsync<Establishment>();
        if (currentSchool == null)
        {
            return (null, []);
        }

        var similarSchools = await results.ReadAsync<Establishment>();
        var performanceDaos = await results.ReadAsync<SimilarSchoolPerformanceDao>();

        var currentSchoolPerformance = performanceDaos.FirstOrDefault(a => a.Id == urn)
            ?? new SimilarSchoolPerformanceDao
            {
                Id = urn,
                Attainment8_Tot_Est_Current_Num = string.Empty,
                Bio59_Sum_Est_Current_Num = string.Empty,
                Chem59_Sum_Est_Current_Num = string.Empty,
                CombSci59_Sum_Est_Current_Num = string.Empty,
                EngLang59_Sum_Est_Current_Num = string.Empty,
                EngLit59_Sum_Est_Current_Num = string.Empty,
                EngMaths59_Tot_Est_Current_Num = string.Empty,
                EngMaths59_Tot_Est_Current_Pct = string.Empty,
                Maths59_Sum_Est_Current_Num = string.Empty,
                Physics59_Sum_Est_Current_Num = string.Empty,
            };
        var similarSchoolsPerformance = performanceDaos.Where(a => a.Id != urn);

        return (
            FromDao(currentSchool, currentSchoolPerformance),
            similarSchools
                .Join(similarSchoolsPerformance, d => d.URN, a => a.Id, FromDao)
                .ToList()
                .AsReadOnly());
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValues>> GetSecondaryValuesByUrnsAsync(
        IEnumerable<string> urns)
    {
        if (urns is null)
        {
            return [];
        }

        var urnList = urns as IList<string> ?? urns.ToList();
        if (urnList.Count == 0)
        {
            return [];
        }

        const string sql = """
            SELECT
                urn                      AS "Urn",
                ks2_rp                   AS "Ks2Rp",
                ks2_mp                   AS "Ks2Mp",
                pp_perc                  AS "PpPerc",
                percent_eal              AS "PercentEal",
                polar4quintile_pupils    AS "Polar4QuintilePupils",
                p_stability              AS "PStability",
                idaci_pupils             AS "IdaciPupils",
                percent_sch_support      AS "PercentSchSupport",
                number_of_pupils         AS "NumberOfPupils",
                percent_statement_or_ehp AS "PercentStatementOrEhp"
            FROM public.v_similar_schools_secondary_values
            WHERE urn = ANY(@Urns);
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();

        var daos = await conn.QueryAsync<SimilarSchoolsSecondaryValuesDao>(
            new CommandDefinition(sql, new { Urns = urnList.ToArray() }));

        return daos
            .Select(d => new SimilarSchoolsSecondaryValues
            {
                Urn = d.Urn,
                Ks2ReadingScore = d.Ks2Rp,
                Ks2MathsScore = d.Ks2Mp,
                PupilPremiumEligibilityPercentage = d.PpPerc,
                PupilsWithEalPercentage = d.PercentEal,
                Polar4Quintile = d.Polar4QuintilePupils,
                PupilStabilityRate = d.PStability,
                AverageIdaciScore = d.IdaciPupils,
                PupilsWithSenSupportPercentage = d.PercentSchSupport,
                PupilCount = d.NumberOfPupils,
                PupilsWithEhcPlanPercentage = d.PercentStatementOrEhp
            })
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviations> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
    {
        const string sql = """
            SELECT
                ks2_avg::numeric(18,6)                  AS "Ks2AverageScore",
                pp_perc::numeric(18,6)                  AS "PpPerc",
                percent_eal::numeric(18,6)              AS "PercentEal",
                polar4quintile_pupils::numeric(18,6)    AS "Polar4QuintilePupils",
                p_stability::numeric(18,6)              AS "PStability",
                idaci_pupils::numeric(18,6)             AS "IdaciPupils",
                percent_sch_support::numeric(18,6)      AS "PercentSchSupport",
                number_of_pupils::numeric(18,6)         AS "NumberOfPupils",
                percent_statement_or_ehp::numeric(18,6) AS "PercentStatementOrEhp"
            FROM public.v_similar_schools_secondary_values_national_sd;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var dao = await conn.QuerySingleAsync<SimilarSchoolsSecondaryNationalSdDao>(sql);
        return new SimilarSchoolsSecondaryStandardDeviations
        {
            Ks2AverageScore = dao.Ks2AverageScore,
            PupilPremiumEligibilityPercentage = dao.PpPerc,
            PupilsWithEalPercentage = dao.PercentEal,
            Polar4Quintile = dao.Polar4QuintilePupils,
            PupilStabilityRate = dao.PStability,
            AverageIdaciScore = dao.IdaciPupils,
            PupilsWithSenSupportPercentage = dao.PercentSchSupport,
            PupilCount = dao.NumberOfPupils,
            PupilsWithEhcPlanPercentage = dao.PercentStatementOrEhp
        };
    }

    private SimilarSchool FromDao(Establishment sch, SimilarSchoolPerformanceDao perf) => new SimilarSchool
    {
        URN = sch.URN,
        Name = sch.EstablishmentName,
        Address = new Address
        {
            Street = sch.Street,
            Locality = sch.Locality,
            Address3 = sch.Address3,
            Town = sch.Town,
            Postcode = sch.Postcode
        },
        TotalCapacity = sch.TotalCapacity,
        TotalPupils = sch.TotalPupils,
        NurseryProvisionName = sch.NurseryProvisionName,
        Coordinates = BNGCoordinates.TryParse(sch.Easting, sch.Northing, out var coords) ? coords : null,
        LocalAuthority = new(sch.LAId, sch.LAName),
        UrbanRural = new(sch.UrbanRuralId, sch.UrbanRuralName),
        Region = new(sch.RegionId, sch.RegionName),
        AdmissionsPolicy = new(sch.AdmissionsPolicyId, sch.AdmissionsPolicyName),
        PhaseOfEducation = new(sch.PhaseOfEducationId, sch.PhaseOfEducationName),
        Gender = new(sch.GenderId, sch.GenderName),
        TypeOfEstablishment = new(sch.TypeOfEstablishmentId, sch.TypeOfEstablishmentName),
        EstablishmentTypeGroup = new(sch.EstablishmentTypeGroupId, sch.EstablishmentTypeGroupName),
        TrustSchoolFlag = new(sch.TrustSchoolFlagId, sch.TrustSchoolFlagName),
        OfficialSixthForm = new(sch.OfficialSixthFormId, sch.OfficialSixthFormName),
        ResourcedProvision = new(sch.ResourcedProvisionId, sch.ResourcedProvisionName),
        Attainment8Score = DataWithAvailability.FromDecimalString(perf.Attainment8_Tot_Est_Current_Num),
        BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Bio59_Sum_Est_Current_Num),
        ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Chem59_Sum_Est_Current_Num),
        CombinedScienceGcseGrade55AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.CombSci59_Sum_Est_Current_Num),
        EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLang59_Sum_Est_Current_Num),
        EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLit59_Sum_Est_Current_Num),
        EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngMaths59_Tot_Est_Current_Num),
        MathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Maths59_Sum_Est_Current_Num),
        PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Physics59_Sum_Est_Current_Num),
    };

    private class SimilarSchoolPerformanceDao
    {
        public required string Id { get; set; }
        public required string Attainment8_Tot_Est_Current_Num { get; set; }
        public required string Bio59_Sum_Est_Current_Num { get; set; }
        public required string Chem59_Sum_Est_Current_Num { get; set; }
        public required string CombSci59_Sum_Est_Current_Num { get; set; }
        public required string EngLang59_Sum_Est_Current_Num { get; set; }
        public required string EngLit59_Sum_Est_Current_Num { get; set; }
        public required string EngMaths59_Tot_Est_Current_Num { get; set; }
        public required string EngMaths59_Tot_Est_Current_Pct { get; set; }
        public required string Maths59_Sum_Est_Current_Num { get; set; }
        public required string Physics59_Sum_Est_Current_Num { get; set; }
    }

    private class SimilarSchoolsSecondaryValuesDao
    {
        public string Urn { get; init; } = string.Empty;
        public decimal Ks2Rp { get; init; }
        public decimal Ks2Mp { get; init; }
        public decimal PpPerc { get; init; }
        public decimal PercentEal { get; init; }
        public int Polar4QuintilePupils { get; init; }
        public decimal PStability { get; init; }
        public decimal IdaciPupils { get; init; }
        public decimal PercentSchSupport { get; init; }
        public int NumberOfPupils { get; init; }
        public decimal PercentStatementOrEhp { get; init; }
    }

    private class SimilarSchoolsSecondaryNationalSdDao
    {
        public required decimal Ks2AverageScore { get; init; }
        public required decimal PpPerc { get; init; }
        public required decimal PercentEal { get; init; }
        public required decimal Polar4QuintilePupils { get; init; }
        public required decimal PStability { get; init; }
        public required decimal IdaciPupils { get; init; }
        public required decimal PercentSchSupport { get; init; }
        public required decimal NumberOfPupils { get; init; }
        public required decimal PercentStatementOrEhp { get; init; }
    }
}
