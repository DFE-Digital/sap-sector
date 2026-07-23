using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Dto.SimilarSchools.Primary;

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

    public static class Ks2Performance
    {
        public static EstablishmentPerformance Establishment(string urn, Func<EstablishmentPerformanceBuilder, EstablishmentPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new EstablishmentPerformanceBuilder(urn)).Build();
        }

        public static LAPerformance LA(string laId, Func<LAPerformanceBuilder, LAPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new LAPerformanceBuilder(laId)).Build();
        }

        public static EnglandPerformance England(Func<EnglandPerformanceBuilder, EnglandPerformanceBuilder>? build = null)
        {
            build ??= b => b;
            return build(new EnglandPerformanceBuilder()).Build();
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
