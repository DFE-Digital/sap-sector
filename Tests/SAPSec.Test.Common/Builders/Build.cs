using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using KS2Performance = SAPSec.Data.Dto.KS2.Performance;
using KS4Destinations = SAPSec.Data.Dto.KS4.Destinations;
using KS4Performance = SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Test.Common.Builders;

public static class Build
{
    public static Establishment Establishment(string urn, string name, Func<EstablishmentBuilder, EstablishmentBuilder>? build = null)
    {
        build ??= b => b;
        return build(new EstablishmentBuilder(urn, name)).Build();
    }

    public static SimilarSchoolsPrimaryGroupsEntry[] PrimaryGroup(string urn, IEnumerable<string> neighbourUrns)
    {
        return neighbourUrns
            .Select(n => new SimilarSchoolsPrimaryGroupsEntry { URN = urn, NeighbourURN = n })
            .ToArray();
    }

    public static SimilarSchoolsPrimaryValuesEntry[] PrimaryValues(params string[] urns)
    {
        return urns
            .Select(urn => new SimilarSchoolsPrimaryValuesEntry { URN = urn })
            .ToArray();
    }

    public static SimilarSchoolsSecondaryGroupsEntry[] SecondaryGroup(string urn, IEnumerable<string> neighbourUrns)
    {
        return neighbourUrns
            .Select(n => new SimilarSchoolsSecondaryGroupsEntry { URN = urn, NeighbourURN = n })
            .ToArray();
    }

    public static SimilarSchoolsSecondaryValuesEntry[] SecondaryValues(params string[] urns)
    {
        return urns
            .Select(urn => new SimilarSchoolsSecondaryValuesEntry { URN = urn })
            .ToArray();
    }

    public static class Ks2Performance
    {
        public static KS2Performance.EstablishmentPerformance Establishment(string urn, Func<KS2.EstablishmentPerformanceBuilder, KS2.EstablishmentPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS2.EstablishmentPerformanceBuilder(urn)).Build();
        }

        public static KS2Performance.LAPerformance LA(string laId, Func<KS2.LAPerformanceBuilder, KS2.LAPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS2.LAPerformanceBuilder(laId)).Build();
        }

        public static KS2Performance.EnglandPerformance England(Func<KS2.EnglandPerformanceBuilder, KS2.EnglandPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS2.EnglandPerformanceBuilder()).Build();
        }
    }

    public static class Ks4Performance
    {
        public static KS4Performance.EstablishmentPerformance Establishment(string urn, Func<KS4.EstablishmentPerformanceBuilder, KS4.EstablishmentPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.EstablishmentPerformanceBuilder(urn)).Build();
        }

        public static KS4Performance.LAPerformance LA(string laId, Func<KS4.LAPerformanceBuilder, KS4.LAPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.LAPerformanceBuilder(laId)).Build();
        }

        public static KS4Performance.EnglandPerformance England(Func<KS4.EnglandPerformanceBuilder, KS4.EnglandPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.EnglandPerformanceBuilder()).Build();
        }
    }

    public static class Ks4Destinations
    {
        public static KS4Destinations.EstablishmentDestinations Establishment(string urn, Func<KS4.EstablishmentDestinationsBuilder, KS4.EstablishmentDestinationsBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.EstablishmentDestinationsBuilder(urn)).Build();
        }

        public static KS4Destinations.LADestinations LA(string laId, Func<KS4.LADestinationsBuilder, KS4.LADestinationsBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.LADestinationsBuilder(laId)).Build();
        }

        public static KS4Destinations.EnglandDestinations England(Func<KS4.EnglandDestinationsBuilder, KS4.EnglandDestinationsBuilder>? build = null)
        {
            build ??= b => b;
            return build(new KS4.EnglandDestinationsBuilder()).Build();
        }
    }

    public static class Absence
    {
        public static EstablishmentAbsence Establishment(string urn, Func<EstablishmentAbsenceBuilder, EstablishmentAbsenceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new EstablishmentAbsenceBuilder(urn)).Build();
        }

        public static LAAbsence LA(string laId, Func<LAAbsenceBuilder, LAAbsenceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new LAAbsenceBuilder(laId)).Build();
        }

        public static EnglandAbsence England(Func<EnglandAbsenceBuilder, EnglandAbsenceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new EnglandAbsenceBuilder()).Build();
        }
    }
}
