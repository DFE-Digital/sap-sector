namespace SAPSec.Core.Model;

/// <summary>
/// Wraps a value with its availability status.
/// Immutable record following value object pattern.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public sealed record DataWithAvailability<T>
{
    public T? Value { get; }
    public DataAvailabilityStatus Availability { get; }

    internal DataWithAvailability(T? value, DataAvailabilityStatus availability)
    {
        Value = value;
        Availability = availability;
    }

    #region Query Methods

    /// <summary>Returns true if data is available</summary>
    public bool IsAvailable => Availability == DataAvailabilityStatus.Available;

    /// <summary>Returns true if data has a usable value (Available or Low quality)</summary>
    public bool HasValue => Availability is DataAvailabilityStatus.Available or DataAvailabilityStatus.Low;

    /// <summary>Returns true if data is explicitly marked as not applicable</summary>
    public bool IsNotApplicable => Availability == DataAvailabilityStatus.NotApplicable;

    #endregion

    #region Transformation Methods

    /// <summary>
    /// Maps the value to a new type while preserving availability.
    /// </summary>
    public DataWithAvailability<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (!HasValue || Value is null)
        {
            return Availability switch
            {
                DataAvailabilityStatus.Redacted => DataAvailability.Redacted<TResult>(),
                DataAvailabilityStatus.NotApplicable => DataAvailability.NotApplicable<TResult>(),
                _ => DataAvailability.NotAvailable<TResult>()
            };
        }

        return DataAvailability.Available(mapper(Value));
    }

    /// <summary>
    /// Gets the value or a default if not available.
    /// </summary>
    public T GetValueOrDefault(T defaultValue)
        => HasValue && Value is not null ? Value : defaultValue;

    #endregion
}