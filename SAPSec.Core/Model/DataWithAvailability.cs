namespace SAPSec.Core.Model;

/// <summary>
/// Wraps a value with its availability status.
/// Immutable record following value object pattern.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public sealed record DataWithAvailability<T>
{
    public T? Value { get; }
    public DataAvailability Availability { get; }

    private DataWithAvailability(T? value, DataAvailability availability)
    {
        Value = value;
        Availability = availability;
    }

    #region Factory Methods - Open for extension via new factory methods

    /// <summary>Creates an available value</summary>
    public static DataWithAvailability<T> Available(T value)
        => new(value, DataAvailability.Available);

    /// <summary>Creates a not available value</summary>
    public static DataWithAvailability<T> NotAvailable()
        => new(default, DataAvailability.NotAvailable);

    /// <summary>Creates a not applicable value</summary>
    public static DataWithAvailability<T> NotApplicable()
        => new(default, DataAvailability.NotApplicable);

    /// <summary>Creates a redacted value</summary>
    public static DataWithAvailability<T> Redacted()
        => new(default, DataAvailability.Redacted);

    /// <summary>Creates a low quality value</summary>
    public static DataWithAvailability<T> Low(T value)
        => new(value, DataAvailability.Low);

    #endregion

    #region Query Methods

    /// <summary>Returns true if data is available</summary>
    public bool IsAvailable => Availability == DataAvailability.Available;

    /// <summary>Returns true if data has a usable value (Available or Low quality)</summary>
    public bool HasValue => Availability is DataAvailability.Available or DataAvailability.Low;

    /// <summary>Returns true if data is explicitly marked as not applicable</summary>
    public bool IsNotApplicable => Availability == DataAvailability.NotApplicable;

    #endregion

    #region Transformation Methods

    /// <summary>
    /// Maps the value to a new type while preserving availability.
    /// Follows functor pattern.
    /// </summary>
    public DataWithAvailability<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (!HasValue || Value is null)
        {
            return Availability switch
            {
                DataAvailability.Redacted => DataWithAvailability<TResult>.Redacted(),
                DataAvailability.NotApplicable => DataWithAvailability<TResult>.NotApplicable(),
                _ => DataWithAvailability<TResult>.NotAvailable()
            };
        }

        return DataWithAvailability<TResult>.Available(mapper(Value));
    }

    /// <summary>
    /// Gets the value or a default if not available.
    /// </summary>
    public T GetValueOrDefault(T defaultValue)
        => HasValue && Value is not null ? Value : defaultValue;

    #endregion
}