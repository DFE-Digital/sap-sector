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
}
