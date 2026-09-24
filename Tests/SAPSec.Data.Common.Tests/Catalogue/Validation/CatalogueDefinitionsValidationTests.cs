using SAPSec.Data.Common.Catalogue;
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
        SourceProfiles.Load(Repository.PathTo("SAPData", "DataMap", "source-profiles.json"));

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

    [Theory]
    [InlineData(Ks4Performance.Type, DataYears.Ks4Performance)]
    [InlineData(Ks4Destinations.Type, DataYears.Ks4Destinations)]
    [InlineData(Ks2Performance.Type, DataYears.Ks2Performance)]
    [InlineData(PupilAbsence.Type, DataYears.PupilAbsence)]
    [InlineData(Workforce.Type, DataYears.Workforce)]
    public void Current_year_matches_the_year_the_website_labels(string type, int year)
    {
        CatalogueDefinitions.Rows()
            .Where(r => r.Type == type && r.YearDesc == nameof(Period.Current))
            .Select(r => r.Year)
            .Distinct()
            .Should().Equal(new AcademicYear(year).Label);
    }
}
