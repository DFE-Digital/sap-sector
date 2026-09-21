using Microsoft.AspNetCore.Html;

namespace SAPSec.Web.TagHelpers;

public sealed record ContentToggleItem(string Id, string Name, string? AriaLabel, IHtmlContent Content, bool Active);
