namespace SAPSec.Core.Model;

public class Role
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int NumericId { get; set; }
    public int Status { get; set; }
}