using Microsoft.AspNetCore.Html;

namespace SAPSec.Web.TagHelpers;

public sealed record ContentToggleItem(string Id, string Name, IHtmlContent Content, bool Active);
