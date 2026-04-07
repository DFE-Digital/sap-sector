using System.Text;

namespace SAPSec.Core;

public class ValidationException : Exception
{
    private List<ValidationError> _errors = new();

    public IEnumerable<ValidationError> Errors => _errors;

    public ValidationException()
    {
    }

    public ValidationException(IEnumerable<ValidationError> errors) : this()
    {
        _errors = errors.ToList();
    }

    public override string Message
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine("There were some issues processing your request:");
            foreach (var error in Errors)
            {
                sb.AppendLine(error.Message);
            }

            return sb.ToString();
        }
    }

    public void AddError(string key, string message) => _errors.Add(new(key, message));
}
