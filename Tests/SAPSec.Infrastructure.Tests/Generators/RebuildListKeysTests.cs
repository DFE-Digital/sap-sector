using FluentAssertions;
using SAPSec.Data.Common;

namespace SAPSec.Infrastructure.Tests.Generators;

public class RebuildListKeysTests
{
    [Fact]
    public void DatedEntry_ResolvesToTheDatedFileKey()
    {
        var result = RebuildListKeys.ExpandDatedKeys(
            ["edubasealldataYYYYmmDD"],
            ["edubasealldata20261124", "ks2_school_attainment_data"]);

        result.Should().BeEquivalentTo(["edubasealldata20261124"]);
    }

    [Fact]
    public void DatedEntry_DoesNotMatchOtherDatasetsThatShareThePrefix()
    {
        var result = RebuildListKeys.ExpandDatedKeys(
            ["edubasealldataYYYYmmDD", "links_edubasealldataYYYYmmDD", "grouplinks_edubaseallacademiesandfreeYYYYmmDD"],
            ["edubasealldata20261124", "links_edubasealldata20261124", "grouplinks_edubaseallacademiesandfree20261124"]);

        result.Should().BeEquivalentTo([
            "edubasealldata20261124",
            "links_edubasealldata20261124",
            "grouplinks_edubaseallacademiesandfree20261124"
        ]);
    }

    [Fact]
    public void DatedEntry_IgnoresKeysThatAreNotAnEightDigitDate()
    {
        var result = RebuildListKeys.ExpandDatedKeys(
            ["edubasealldataYYYYmmDD"],
            ["edubasealldata202611", "edubasealldata20261124_old", "edubasealldataYYYYmmDD"]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DatedEntry_WithNoMatchingFile_IsDroppedAndLogged()
    {
        var log = new List<string>();

        var result = RebuildListKeys.ExpandDatedKeys(["edubasealldataYYYYmmDD"], ["ks2_school_attainment_data"], log.Add);

        result.Should().BeEmpty();
        log.Should().ContainSingle().Which.Should().Contain("matched no downloaded file");
    }

    [Fact]
    public void UndatedEntries_AreKeptUnchanged()
    {
        var result = RebuildListKeys.ExpandDatedKeys(
            ["ks2_school_attainment_data", "exc_school"],
            []);

        result.Should().BeEquivalentTo(["ks2_school_attainment_data", "exc_school"]);
    }

    [Fact]
    public void Matching_IsCaseInsensitive()
    {
        var result = RebuildListKeys.ExpandDatedKeys(["EdubaseAllDataYYYYMMDD"], ["edubasealldata20261124"]);

        result.Should().BeEquivalentTo(["edubasealldata20261124"]);
    }

    [Theory]
    [InlineData("C:/data/manual_ks2_school_attainment_data.csv", "ks2_school_attainment_data")]
    [InlineData("C:/data/edubasealldata20261124.csv", "edubasealldata20261124")]
    [InlineData("MANUAL_exc_school.csv", "exc_school")]
    public void LogicalKeyForFile_StripsExtensionAndManualPrefix(string path, string expected)
    {
        RebuildListKeys.LogicalKeyForFile(path).Should().Be(expected);
    }
}
