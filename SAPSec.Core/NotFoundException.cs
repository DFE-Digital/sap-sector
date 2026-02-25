namespace SAPSec.Core;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}