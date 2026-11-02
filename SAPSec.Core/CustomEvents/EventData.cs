namespace SAPSec.Core.CustomEvents;

public class EventData
{
    public string EventType { get; set; } = string.Empty;
    public string LinkText { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public string DetailsSectionName { get; set; } = string.Empty;
    public string DetailsSectionLabel { get; set; } = string.Empty;
    public string DetailsInteractionType { get; set; } = string.Empty;

}
