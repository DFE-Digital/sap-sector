using FluentAssertions;
using SAPSec.Core.Features.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Measures.Primary;

public class PlaceholderKs2ProgressPupilNumbersProviderTests
{
    private readonly PlaceholderKs2ProgressPupilNumbersProvider _sut = new();

    [Fact]
    public async Task GetAsync_ReturnsAnEntryForEachDistinctUrn()
    {
        var result = await _sut.GetAsync(["100001", "100002", "100001"], Ks2ProgressSubject.Reading, "tot");

        result.Keys.Should().BeEquivalentTo(["100001", "100002"]);
        result["100001"].Urn.Should().Be("100001");
    }

    [Fact]
    public async Task GetAsync_IsStableForAUrn()
    {
        var first = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Maths, "tot");
        var second = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Maths, "tot");

        second["100001"].Should().BeEquivalentTo(first["100001"]);
    }

    [Fact]
    public async Task GetAsync_AllPupils_ReturnsProportionForEachYear()
    {
        var result = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Reading, "tot");

        var proportion = result["100001"].ProportionIncluded;
        proportion.Should().NotBeNull();
        new[] { proportion!.Previous2, proportion.Previous, proportion.Current }
            .Should().AllSatisfy(p => p.Should().BeInRange(0m, 100m));
    }

    [Theory]
    [InlineData("boy")]
    [InlineData("dis")]
    [InlineData("eal")]
    [InlineData("nmo")]
    [InlineData("hpa")]
    public async Task GetAsync_PupilCharacteristic_HasEligiblePupilsButNoProportion(string characteristic)
    {
        var result = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Reading, characteristic);

        result["100001"].ProportionIncluded.Should().BeNull();
        result["100001"].EligiblePupils.Current.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAsync_EligiblePupilsAreTheSameForEverySubject()
    {
        var reading = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Reading, "tot");
        var writing = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Writing, "tot");
        var maths = await _sut.GetAsync(["100001"], Ks2ProgressSubject.Maths, "tot");

        writing["100001"].EligiblePupils.Should().Be(reading["100001"].EligiblePupils);
        maths["100001"].EligiblePupils.Should().Be(reading["100001"].EligiblePupils);
    }
}
