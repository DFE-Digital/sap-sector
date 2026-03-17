namespace SAPSec.Web.ViewModels;

public class ErrorViewModel
{
    public string? ErrorCode { get; set; }
    public bool ShowErrorCode => !string.IsNullOrEmpty(ErrorCode);

    public string? ErrorMessage { get; set; }
    public bool ShowErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
}
