using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchoolsTests
{
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly FindPrimarySimilarSchools _sut;

    public FindPrimarySimilarSchoolsTests()
    {
        _sut = new FindPrimarySimilarSchools(_establishmentRepo, _similarSchoolsRepo);
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(new("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenCurrentSchoolValuesDoNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(new Establishment { URN = "100001", EstablishmentName = "Current School" });

        var act = async () => await _sut.Execute(new("100001"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100001*");
    }

    [Fact]
    public async Task ReturnsCurrentSchoolAndSimilarSchoolsWithCharacteristics()
    {
        _establishmentRepo.SetupEstablishments(
            new Establishment { URN = "100001", EstablishmentName = "Current School", LAName = "LA One" },
            new Establishment { URN = "100002", EstablishmentName = "Similar School 1", LAName = "LA Two" },
            new Establishment { URN = "100003", EstablishmentName = "Similar School 2", LAName = "LA Three" });

        _similarSchoolsRepo.SetupGroups(
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100002", Dist = "0.1", Rank = "1" },
            new SimilarSchoolsPrimaryGroupsEntry { URN = "100001", NeighbourURN = "100003", Dist = "0.2", Rank = "2" });

        _similarSchoolsRepo.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100001",
                PPPerc = "20.2",
                Polar4QuintilePupils = "2",
                PStability = "95",
                PercentSchSupport = "10",
                PercentEAL = "5",
                IdaciPupils = "0.123",
                PercentageStatementOrEhp = "1.5",
                NumberOfPupils = "210",
                ReadMatAverage = "102.4",
                Ks1PriorRwmAverage = "11.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100002",
                PPPerc = "25.4",
                Polar4QuintilePupils = "3",
                PStability = "94",
                PercentSchSupport = "12",
                PercentEAL = "6",
                IdaciPupils = "0.223",
                PercentageStatementOrEhp = "2.5",
                NumberOfPupils = "220",
                ReadMatAverage = "101.4",
                Ks1PriorRwmAverage = "10.4"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100003",
                PPPerc = "30.5",
                Polar4QuintilePupils = "4",
                PStability = "93",
                PercentSchSupport = "14",
                PercentEAL = "7",
                IdaciPupils = "0.323",
                PercentageStatementOrEhp = "3.5",
                NumberOfPupils = "230",
                ReadMatAverage = "100.4",
                Ks1PriorRwmAverage = "9.4"
            });

        var response = await _sut.Execute(new("100001"));

        response.CurrentSchool.Should().BeEquivalentTo(new
        {
            Urn = "100001",
            Name = "Current School",
            LocalAuthorityName = "LA One"
        });
        response.CurrentSchool.Characteristics.ReadMatAverage.Should().Be(102.4m);
        response.CurrentSchool.Characteristics.Ks1PriorRwmAverage.Should().Be(11.4m);

        response.SimilarSchools.Should().HaveCount(2);
        response.SimilarSchools.Should().SatisfyRespectively(
            first =>
            {
                first.Urn.Should().Be("100002");
                first.Name.Should().Be("Similar School 1");
                first.LocalAuthorityName.Should().Be("LA Two");
                first.Rank.Should().Be("1");
                first.Distance.Should().Be("0.1");
                first.Characteristics.PupilCount.Should().Be(220m);
            },
            second =>
            {
                second.Urn.Should().Be("100003");
                second.Name.Should().Be("Similar School 2");
                second.LocalAuthorityName.Should().Be("LA Three");
                second.Rank.Should().Be("2");
                second.Distance.Should().Be("0.2");
                second.Characteristics.PupilPremiumEligibilityPercentage.Should().Be(30.5m);
            });
    }
}
