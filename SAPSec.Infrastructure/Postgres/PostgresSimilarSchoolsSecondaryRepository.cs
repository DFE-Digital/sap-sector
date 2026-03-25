using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

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
            SELECT "NeighbourURN" 
            FROM public.v_similar_schools_secondary_groups 
            WHERE "URN" = @urn;
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
                SELECT "NeighbourURN" 
                FROM public.v_similar_schools_secondary_groups 
                WHERE "URN" = @urn
            );

            SELECT
                "Id",
                "Attainment8_Tot_Est_Current_Num",
                "Bio59_Sum_Est_Current_Pct",
                "Chem59_Sum_Est_Current_Pct",
                "CombSci59_Sum_Est_Current_Pct",
                "EngLang59_Sum_Est_Current_Pct",
                "EngLit59_Sum_Est_Current_Pct",
                "EngMaths59_Tot_Est_Current_Pct",
                "Maths59_Sum_Est_Current_Pct",
                "Physics59_Sum_Est_Current_Pct"
            FROM public.v_establishment_performance
            WHERE "Id" = @urn OR "Id" IN (
                SELECT "NeighbourURN" 
                FROM public.v_similar_schools_secondary_groups 
                WHERE "URN" = @urn
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
                Bio59_Sum_Est_Current_Pct = string.Empty,
                Chem59_Sum_Est_Current_Pct = string.Empty,
                CombSci59_Sum_Est_Current_Pct = string.Empty,
                EngLang59_Sum_Est_Current_Pct = string.Empty,
                EngLit59_Sum_Est_Current_Pct = string.Empty,
                EngMaths59_Tot_Est_Current_Pct = string.Empty,
                Maths59_Sum_Est_Current_Pct = string.Empty,
                Physics59_Sum_Est_Current_Pct = string.Empty,
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
                "URN",
                "KS2RP",
                "KS2MP",
                "PPPerc",
                "PercentEAL",
                "Polar4QuintilePupils",
                "PStability",
                "IdaciPupils",
                "PercentSchSupport",
                "NumberOfPupils",
                "PercentageStatementOrEHP"
            FROM public.v_similar_schools_secondary_values
            WHERE "URN" = ANY(@Urns);
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();

        var daos = await conn.QueryAsync<SimilarSchoolsSecondaryValuesEntry>(
            new CommandDefinition(sql, new { Urns = urnList.ToArray() }));

        return daos
            .Select(d => new SimilarSchoolsSecondaryValues
            {
                Urn = d.URN,
                Ks2ReadingScore = ParseDecimal(d.KS2RP),
                Ks2MathsScore = ParseDecimal(d.KS2MP),
                PupilPremiumEligibilityPercentage = ParseDecimal(d.PPPerc),
                PupilsWithEalPercentage = ParseDecimal(d.PercentEAL),
                Polar4Quintile = ParseDecimal(d.Polar4QuintilePupils),
                PupilStabilityRate = ParseDecimal(d.PStability),
                AverageIdaciScore = ParseDecimal(d.IdaciPupils),
                PupilsWithSenSupportPercentage = ParseDecimal(d.PercentSchSupport),
                PupilCount = ParseDecimal(d.NumberOfPupils),
                PupilsWithEhcPlanPercentage = ParseDecimal(d.PercentageStatementOrEHP)
            })
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviations> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
    {
        const string sql = """
            SELECT
                "KS2AVG",
                "PPPerc",
                "PercentEAL",
                "Polar4QuintilePupils",
                "PStability",
                "IdaciPupils",
                "PercentSchSupport",
                "NumberOfPupils",
                "PercentageStatementOrEHP"
            FROM public.v_similar_schools_secondary_values_national_sd;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var dao = await conn.QuerySingleAsync<SimilarSchoolsSecondaryStandardDeviationsEntry>(sql);
        return new SimilarSchoolsSecondaryStandardDeviations
        {
            Ks2AverageScore = dao.KS2AVG,
            PupilPremiumEligibilityPercentage = dao.PPPerc,
            PupilsWithEalPercentage = dao.PercentEAL,
            Polar4Quintile = dao.Polar4QuintilePupils,
            PupilStabilityRate = dao.PStability,
            AverageIdaciScore = dao.IdaciPupils,
            PupilsWithSenSupportPercentage = dao.PercentSchSupport,
            PupilCount = dao.NumberOfPupils,
            PupilsWithEhcPlanPercentage = dao.PercentageStatementOrEHP
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
        BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Bio59_Sum_Est_Current_Pct),
        ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Chem59_Sum_Est_Current_Pct),
        CombinedScienceGcseGrade55AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.CombSci59_Sum_Est_Current_Pct),
        EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLang59_Sum_Est_Current_Pct),
        EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLit59_Sum_Est_Current_Pct),
        EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngMaths59_Tot_Est_Current_Pct),
        MathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Maths59_Sum_Est_Current_Pct),
        PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Physics59_Sum_Est_Current_Pct),
    };

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        return decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 0m;
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return int.TryParse(
            value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 0;
    }

    private class SimilarSchoolPerformanceDao
    {
        public required string Id { get; set; }
        public required string Attainment8_Tot_Est_Current_Num { get; set; }
        public required string Bio59_Sum_Est_Current_Pct { get; set; }
        public required string Chem59_Sum_Est_Current_Pct { get; set; }
        public required string CombSci59_Sum_Est_Current_Pct { get; set; }
        public required string EngLang59_Sum_Est_Current_Pct { get; set; }
        public required string EngLit59_Sum_Est_Current_Pct { get; set; }
        public required string EngMaths59_Tot_Est_Current_Pct { get; set; }
        public required string Maths59_Sum_Est_Current_Pct { get; set; }
        public required string Physics59_Sum_Est_Current_Pct { get; set; }
    }
}
