using SAPSec.Data.Common.Catalogue.Definitions;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPSec.Data.Common.Tests.Catalogue.Validation;

/// <summary>
/// Checks every dataset defined in code against the committed snapshot of its source files.
/// If this fails after adding or changing a source file, regenerate the snapshot with the source files in
/// SAPData/DataMap/SourceFiles: dotnet run --project SAPData -- profile-sources
/// </summary>
public class CatalogueDefinitionsValidationTests
{
    private static readonly SourceProfiles Profiles =
        SourceProfiles.Load(DataMapCsv.RepositoryPath("SAPData", "DataMap", "source-profiles.json"));

    [Fact]
    public void Every_definition_is_valid_against_its_source_files()
    {
        CatalogueValidator.Validate(CatalogueDefinitions.Rows(), Profiles)
            .Select(i => i.ToString())
            .Should().BeEmpty();
    }

    [Fact]
    public void Types_lists_every_type_the_definitions_produce()
    {
        CatalogueDefinitions.Rows().Select(r => r.Type).Distinct()
            .Should().BeEquivalentTo(CatalogueDefinitions.Types);
    }

    /// <summary>
    /// The validator would have caught every KS4 error found in datamap.csv. Remove with datamap.csv (Story 3).
    /// </summary>
    [Theory]
    [InlineData(CatalogueValidator.PeriodInName, "Prog8_Avg_LA_Previous2_Num")]
    [InlineData(CatalogueValidator.DuplicateMapping, "EngMaths59_Mob_Eng_Current_Pct")]
    [InlineData(CatalogueValidator.UnknownValue, "EngLang79_Sum_Est_Previous_Pct")]
    [InlineData(CatalogueValidator.UnknownValue, "Maths79_Sum_Est_Previous_Pct")]
    [InlineData(CatalogueValidator.UnknownValue, "Attainment8_EAL_Est_Current_Num")]
    [InlineData(CatalogueValidator.UnknownValue, "Attainment8_NDi_Est_Previous_Num")]
    public void Catches_the_errors_found_in_the_original_ks4_data_map(string rule, string property)
    {
        var issues = CatalogueValidator.Validate(DataMapCsv.ForType(Ks4Performance.Type), Profiles);

        issues.Should().Contain(i => i.Rule == rule && i.Property == property);
    }
}
