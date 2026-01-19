using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Rules;

/// <summary>
/// Interface for business rules that determine a value from establishment data.
/// Follows Strategy pattern - allows swapping rule implementations.
/// </summary>
/// <typeparam name="T">The type of value the rule produces</typeparam>
public interface IBusinessRule<T>
{
    /// <summary>
    /// Evaluates the rule against an establishment.
    /// </summary>
    /// <param name="establishment">The establishment to evaluate</param>
    /// <returns>The result with availability information</returns>
    DataWithAvailability<T> Evaluate(Establishment establishment);
}