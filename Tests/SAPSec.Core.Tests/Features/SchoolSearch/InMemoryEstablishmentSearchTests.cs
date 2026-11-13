using FluentAssertions;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Data.Dto;
using Xunit;

namespace SAPSec.Core.Tests.Features.SchoolSearch;

public class InMemoryEstablishmentSearchTests
{
    private static Establishment School(string urn, string name, string street = "", string postcode = "", string phaseId = "4")
        => new() { URN = urn, EstablishmentName = name, Street = street, Postcode = postcode, PhaseOfEducationId = phaseId };

    private static readonly Establishment[] Schools =
    [
        School("1", "St Mary's School", "High Road", "SW1A 1AA"),
        School("2", "Fake School Three", "Mount Lane", "AB1 2CD"),
        School("3", "Saint Peter CofE School", "Park Avenue", "ZZ9 9ZZ"),
        School("4", "Nursery Only", "", "", phaseId: "1"),
        School("5", "St Joseph's RC Primary", "Church Street", "XX1 1XX")
    ];

    private static IEnumerable<string> Urns(string query)
        => InMemoryEstablishmentSearch.Search(Schools, query, 10).Select(h => h.URN);

    [Fact]
    public void BlankQuery_ReturnsNothing() => Urns("  ").Should().BeEmpty();

    [Theory]
    [InlineData("st mary", "1")]
    [InlineData("saint mary", "1")]
    [InlineData("fa", "2")]
    [InlineData("three fake", "2")]
    [InlineData("church of england school", "3")]
    [InlineData("sw1a1aa", "1")]
    [InlineData("sw1a 1", "1")]
    [InlineData("mt lane", "2")]
    [InlineData("catholic primary", "5")]
    [InlineData("roman catholic joseph", "5")]
    [InlineData("rc joseph", "5")]
    public void Matches(string query, string expectedUrn) => Urns(query).Should().Contain(expectedUrn);

    [Fact]
    public void ExcludesPhasesThatCannotBeIndexed() => Urns("nursery").Should().BeEmpty();
}
