using System.ComponentModel.DataAnnotations;

namespace SAPSec.Web.ViewModels;

public class SchoolSearchQueryViewModel
{
    [Required(ErrorMessage = "Enter a school name or URN to start a search")]
    [MinLength(3, ErrorMessage = "Enter a school name or URN (minimum 3 characters)")]
    public string Query { get; set; } = string.Empty;

    public string? EstablishmentId { get; set; }

    public string Hint => "Search by name, address, postcode or unique reference number (URN)";
}