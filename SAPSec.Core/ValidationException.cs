namespace SAPSec.Core;

public class ValidationException : Exception
{
    private List<string> _errors = new();
    public IEnumerable<string> Errors => _errors;

    public ValidationException() : base("There were some issues processing your request")
    {
    }

    public void AddError(string error) => _errors.Add(error);
}