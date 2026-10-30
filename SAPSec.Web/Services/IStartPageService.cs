namespace SAPSec.Web.Services;

/// <summary>
/// Where the service sends people to start: its "Find a school" search while that is enabled for everyone,
/// otherwise the page that routes each user to the right place.
/// </summary>
public interface IStartPageService
{
    Task<string> GetStartUrlAsync(bool signedIn = true);
}
