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
                "UrbanRuralName",
                "RegionId",
                "RegionName",
                "TotalCapacity",
                "TotalPupils",
                "PhaseOfEducationId",
                "PhaseOfEducationName",
                "NurseryProvisionName",
                "OfficialSixthFormId",
                "OfficialSixthFormName",
                "AdmissionsPolicyId",
                "AdmissionsPolicyName",
                "GenderId",
                "GenderName",
                "ResourcedProvisionId",
                "ResourcedProvisionName",
                "TypeOfEstablishmentId",
                "TypeOfEstablishmentName",
                "EstablishmentTypeGroupId",
                "EstablishmentTypeGroupName",
                "TrustSchoolFlagId",
                "TrustSchoolFlagName"
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
                "UrbanRuralName",
                "RegionId",
                "RegionName",
                "TotalCapacity",
                "TotalPupils",
                "PhaseOfEducationId",
                "PhaseOfEducationName",
                "NurseryProvisionName",
                "OfficialSixthFormId",
                "OfficialSixthFormName",
                "AdmissionsPolicyId",
                "AdmissionsPolicyName",
                "GenderId",
                "GenderName",
                "ResourcedProvisionId",
                "ResourcedProvisionName",
                "TypeOfEstablishmentId",
                "TypeOfEstablishmentName",
                "EstablishmentTypeGroupId",
                "EstablishmentTypeGroupName",
                "TrustSchoolFlagId",
                "TrustSchoolFlagName"
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

        var currentSchoolDao = await results.ReadSingleOrDefaultAsync<SimilarSchoolDao>();
        var similarSchoolDaos = await results.ReadAsync<SimilarSchoolDao>();
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

        if (currentSchoolDao == null)
        {
            return (BuildEmptySchool(urn, currentSchoolPerformance), []);
        }

        var map = SqlMapper.GetTypeMap(typeof(SimilarSchoolDao));
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
        RegionId = sch.RegionId,
        RegionName = sch.RegionName,
        AdmissionsPolicyId = sch.AdmissionsPolicyId,
        AdmissionsPolicyName = sch.AdmissionsPolicyName,
        PhaseOfEducationId = sch.PhaseOfEducationId,
        PhaseOfEducationName = sch.PhaseOfEducationName,
        GenderId = sch.GenderId,
        GenderName = sch.GenderName,
        TotalCapacity = sch.TotalCapacity,
        TotalPupils = sch.TotalPupils,
        TypeOfEstablishmentId = sch.TypeOfEstablishmentId,
        TypeOfEstablishmentName = sch.TypeOfEstablishmentName,
        EstablishmentTypeGroupId = sch.EstablishmentTypeGroupId,
        EstablishmentTypeGroupName = sch.EstablishmentTypeGroupName,
        TrustSchoolFlagId = sch.TrustSchoolFlagId,
        TrustSchoolFlagName = sch.TrustSchoolFlagName,
        OfficialSixthFormId = sch.OfficialSixthFormId,
        OfficialSixthFormName = sch.OfficialSixthFormName,
        NurseryProvisionName = sch.NurseryProvisionName,
        ResourcedProvisionId = sch.ResourcedProvisionId,
        ResourcedProvisionName = sch.ResourcedProvisionName,
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

    private static SimilarSchool BuildEmptySchool(string urn, SimilarSchoolPerformanceDao perf) => new SimilarSchool
    {
        URN = urn,
        Name = string.Empty,
        Address = new Address
        {
            Street = string.Empty,
            Locality = string.Empty,
            Address3 = string.Empty,
            Town = string.Empty,
            Postcode = string.Empty
        },
        LocalAuthority = new Core.Features.SimilarSchools.LocalAuthority(string.Empty, string.Empty),
        Coordinates = null,
        UrbanRuralId = string.Empty,
        UrbanRuralName = string.Empty,
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
        public required string RegionId { get; set; }
        public required string RegionName { get; set; }
        public required string UrbanRuralId { get; set; }
        public required string UrbanRuralName { get; set; }
        public required string TotalCapacity { get; set; }
        public required string TotalPupils { get; set; }
        public required string PhaseOfEducationId { get; set; }
        public required string PhaseOfEducationName { get; set; }
        public required string NurseryProvisionName { get; set; }
        public required string OfficialSixthFormId { get; set; }
        public required string OfficialSixthFormName { get; set; }
        public required string AdmissionsPolicyId { get; set; }
        public required string AdmissionsPolicyName { get; set; }
        public required string GenderId { get; set; }
        public required string GenderName { get; set; }
        public required string ResourcedProvisionId { get; set; }
        public required string ResourcedProvisionName { get; set; }
        public required string TypeOfEstablishmentId { get; set; }
        public required string TypeOfEstablishmentName { get; set; }
        public required string EstablishmentTypeGroupId { get; set; }
        public required string EstablishmentTypeGroupName { get; set; }
        public required string TrustSchoolFlagId { get; set; }
        public required string TrustSchoolFlagName { get; set; }
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
