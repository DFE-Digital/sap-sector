namespace SAPSec.Web.ViewModels.Components;

public class CookiesViewModel(string cookieName, string bannerState)
{
    public string CookieName => cookieName;
    public string BannerState => bannerState;
}
