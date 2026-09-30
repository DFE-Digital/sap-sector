namespace SAPSec.Web.ViewModels;

public record AllThroughSimilarSchoolPhases(bool HasPrimary, bool HasSecondary)
{
    public bool HasAny => HasPrimary || HasSecondary;
}
