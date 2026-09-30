namespace SAPSec.Core.Pagination;

public class PaginationItem
{
    public int Number { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsEllipsis { get; set; }
}