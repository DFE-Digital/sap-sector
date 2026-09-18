namespace SAPSec.Web.ViewModels.Components;

public enum ClosedSchoolBannerVariant
{
    ThisSchool,
    Comparator
}

public record ClosedSchoolBannerViewModel(ClosedSchoolBannerVariant Variant);
