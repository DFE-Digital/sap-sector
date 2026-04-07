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

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_secondary_groups 
            WHERE "URN" = @urn
        """;

        var results = await conn.QueryAsync<SimilarSchoolsSecondaryGroupsEntry>(sql, new { urn });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(IEnumerable<string> urns)
    {
        if (!urns.Any())
        {
            return [];
        }

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_secondary_values
            WHERE "URN" = ANY(@urns);
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();

        var results = await conn.QueryAsync<SimilarSchoolsSecondaryValuesEntry>(sql, new { urns = urns.ToArray() });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
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
        var result = await conn.QuerySingleOrDefaultAsync<SimilarSchoolsSecondaryStandardDeviationsEntry>(sql);

        return result;
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
