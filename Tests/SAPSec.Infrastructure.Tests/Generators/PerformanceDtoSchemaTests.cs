using SAPData.Models;
using SAPSec.DtoGenerator;

namespace SAPSec.Infrastructure.Tests.Generators;

public class PerformanceDtoSchemaTests
{
    [Theory]
    [InlineData("Establishment")]
    [InlineData("LA")]
    [InlineData("England")]
    public void IncludesMappedFieldsAbsentFromSnapshot(string range)
    {
        Dictionary<string, object?> properties = new() { ["Id"] = "123456", ["Existing"] = "c" };
        DataMapRow[] rows =
        [
            new() { Range = range, Type = "KS4_Performance", PropertyName = "HistoricalValue" },
            new() { Range = range, Type = "KS4_Performance", PropertyName = "HistoricalValue" },
            new() { Range = range, Type = "KS4_Performance", PropertyName = "Existing" },
            new() { Range = range, Type = "KS4_Performance", PropertyName = "Ignored", IgnoreMapping = " y " },
            new() { Range = "Other", Type = "KS4_Performance", PropertyName = "OtherRange" },
            new() { Range = range, Type = "KS2_Performance", PropertyName = "OtherPhase" },
            new() { Range = range, Type = "KS4_Performance", PropertyName = " " }
        ];

        PerformanceDtoSchema.IncludeMappedProperties(properties, rows, range + "Performance", "KS4\\Performance");

        properties.Keys.Should().Equal("Id", "Existing", "HistoricalValue");
        properties["Existing"].Should().Be("c");
        properties["HistoricalValue"].Should().BeNull();
    }

    [Fact]
    public void PrimaryPerformanceUsesPrimaryMappings()
    {
        Dictionary<string, object?> properties = new();
        DataMapRow[] rows = [new() { Range = "Establishment", Type = "KS2_Performance", PropertyName = "PrimaryMetric" }];

        PerformanceDtoSchema.IncludeMappedProperties(properties, rows, "EstablishmentPerformance", "KS2\\Performance");

        properties.Keys.Should().Equal("PrimaryMetric");
    }

    [Fact]
    public void SubjectEntryModelsKeepTheirExistingSchema()
    {
        Dictionary<string, object?> properties = new() { ["subject"] = "Chemistry" };

        PerformanceDtoSchema.IncludeMappedProperties(properties, [], "EstablishmentSubjectEntries", "KS4\\Performance");

        properties.Keys.Should().Equal("subject");
    }

    [Fact]
    public void MissingMappingsFailRatherThanSilentlyUsingAnOutdatedSnapshot()
    {
        var act = () => PerformanceDtoSchema.IncludeMappedProperties(
            new Dictionary<string, object?>(), [], "EstablishmentPerformance", "KS4\\Performance");

        act.Should().Throw<InvalidOperationException>();
    }
}
