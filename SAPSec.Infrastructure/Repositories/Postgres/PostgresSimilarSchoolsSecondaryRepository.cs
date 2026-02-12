using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Factories;

namespace SAPSec.Infrastructure.Repositories.Postgres;

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

    public async Task<(SimilarSchool, IReadOnlyCollection<SimilarSchool>)> GetSimilarSchoolsGroupAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT
                "URN",
                "EstablishmentName",
                "LAId",
                "LAName",
                "Street",
                "Locality",
                "Address3",
                "Town",
                "Postcode",
                "Easting",
                "Northing",
                "UrbanRuralId",
                "UrbanRuralName"
            FROM public.v_establishment 
            WHERE "URN" = @urn;
            
            SELECT
                "URN",
                "EstablishmentName",
                "LAId",
                "LAName",
                "Street",
                "Locality",
                "Address3",
                "Town",
                "Postcode",
                "Easting",
                "Northing",
                "UrbanRuralId",
                "UrbanRuralName"
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

        var currentSchoolDao = await results.ReadSingleAsync<SimilarSchoolDao>();
        var similarSchoolDaos = await results.ReadAsync<SimilarSchoolDao>();
        var performanceDaos = await results.ReadAsync<SimilarSchoolPerformanceDao>();

        var currentSchoolPerformance = performanceDaos.First(a => a.Id == urn);
        var similarSchoolsPerformance = performanceDaos.Except([currentSchoolPerformance]);

        return (
            FromDao(currentSchoolDao, currentSchoolPerformance),
            similarSchoolDaos
                .Join(similarSchoolsPerformance, d => d.URN, a => a.Id, FromDao)
                .ToList()
                .AsReadOnly());
    }

    private SimilarSchool FromDao(SimilarSchoolDao sch, SimilarSchoolPerformanceDao perf) => new SimilarSchool
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
        LocalAuthority = new Core.Features.SimilarSchools.LocalAuthority(sch.LAId, sch.LAName),
        Coordinates = BNGCoordinates.TryParse(sch.Easting, sch.Northing, out var coords) ? coords : null,
        UrbanRuralId = sch.UrbanRuralId,
        UrbanRuralName = sch.UrbanRuralName,
        Attainment8Score = DataWithAvailability.FromDecimalString(perf.Attainment8_Tot_Est_Current_Num),
        BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Bio59_Sum_Est_Current_Num),
        ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Chem59_Sum_Est_Current_Num),
        CombinedSciencGcseGrade55AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.CombSci59_Sum_Est_Current_Num),
        EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLang59_Sum_Est_Current_Num),
        EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngLit59_Sum_Est_Current_Num),
        EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.EngMaths59_Tot_Est_Current_Num),
        MathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Maths59_Sum_Est_Current_Num),
        PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(perf.Physics59_Sum_Est_Current_Num),
    };

    private class SimilarSchoolDao
    {
        public required string URN { get; set; }
        public required string EstablishmentName { get; set; }
        public required string Street { get; set; }
        public required string Locality { get; set; }
        public required string Address3 { get; set; }
        public required string Town { get; set; }
        public required string Postcode { get; set; }
        public required string LAId { get; set; }
        public required string LAName { get; set; }
        public required string Easting { get; set; }
        public required string Northing { get; set; }
        public required string UrbanRuralId { get; set; }
        public required string UrbanRuralName { get; set; }
    }

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
}