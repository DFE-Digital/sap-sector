using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public class LinkedSchool
    : School
{
    public bool ShowClosedSchoolBanner { get; private init; }
    public IReadOnlyList<School> Successors { get; private init; }
    public IReadOnlyList<School> Predecessors { get; private init; }

    private LinkedSchool(
        School school,
        bool showClosedSchoolBanner,
        IReadOnlyList<School> successors,
        IReadOnlyList<School> predecessors)
        : base(school)
    {
        ShowClosedSchoolBanner = showClosedSchoolBanner;
        Successors = successors;
        Predecessors = predecessors;
    }

    protected LinkedSchool(LinkedSchool school)
        : base(school)
    {
        ShowClosedSchoolBanner = school.ShowClosedSchoolBanner;
        Successors = school.Successors;
        Predecessors = school.Predecessors;
    }

    public static LinkedSchool Create(
        Establishment establishment,
        SchoolClosureEligibility closureEligibility,
        SchoolPredecessorRelationship predecessorRelationship)
    {
        return new LinkedSchool(
            Create(establishment),
            closureEligibility.ShowClosedSchoolBanner,
            closureEligibility.Successors,
            predecessorRelationship.Predecessors);
    }
}