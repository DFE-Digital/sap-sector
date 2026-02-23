using SAPSec.Core.Constants;

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
                DataAvailabilityStatus.Redacted => DataWithAvailability.Redacted<TResult>(),
                DataAvailabilityStatus.NotApplicable => DataWithAvailability.NotApplicable<TResult>(),
                _ => DataWithAvailability.NotAvailable<TResult>()
            };
        }

        return DataWithAvailability.Available(mapper(Value));
    }

    /// <summary>
    /// Gets the value or a default if not available.
    /// </summary>
    public T GetValueOrDefault(T defaultValue)
        => HasValue && Value is not null ? Value : defaultValue;

    #endregion

    public static readonly DataWithAvailabilityComparer Comparer = new DataWithAvailabilityComparer();

    public class DataWithAvailabilityComparer : IComparer<DataWithAvailability<T>>
    {
        public int Compare(DataWithAvailability<T>? x, DataWithAvailability<T>? y)
        {
            if ((x == null || !x.IsAvailable) && (y == null || !y.IsAvailable))
            {
                return 0;
            }

            if (x != null && x.HasValue && y != null && y.HasValue)
            {
                return Comparer<T>.Default.Compare(x.Value, y.Value);
            }

            // Not available data is arbitrarily valued as less than available data
            if (x == null || !x.IsAvailable)
            {
                return -1;
            }

            return 1;
        }
    }
}

/// <summary>
/// Static factory methods for creating DataWithAvailability instances.
/// Allows type inference to reduce boilerplate.
/// </summary>
/// <example>
/// // Instead of:
/// var city = DataWithAvailability&lt;string&gt;.Available("Sheffield");
/// 
/// // You can write:
/// var city = DataAvailability.Available("Sheffield");
/// var country = DataAvailability.NotAvailable&lt;string&gt;();
/// </example>
public static class DataWithAvailability
{
    /// <summary>
    /// Creates an available value. Type is inferred from the value.
    /// </summary>
    /// <example>
    /// var name = DataAvailability.Available("Test School");
    /// var age = DataAvailability.Available(11);
    /// </example>
    public static DataWithAvailability<T> Available<T>(T value)
        => new(value, DataAvailabilityStatus.Available);

    /// <summary>
    /// Creates a not available value. Type must be specified.
    /// </summary>
    /// <example>
    /// var missing = DataAvailability.NotAvailable&lt;string&gt;();
    /// </example>
    public static DataWithAvailability<T> NotAvailable<T>()
        => new(default, DataAvailabilityStatus.NotAvailable);

    /// <summary>
    /// Creates a not applicable value. Type must be specified.
    /// </summary>
    /// <example>
    /// var notApplicable = DataAvailability.NotApplicable&lt;string&gt;();
    /// </example>
    public static DataWithAvailability<T> NotApplicable<T>()
        => new(default, DataAvailabilityStatus.NotApplicable);

    /// <summary>
    /// Creates a redacted value. Type must be specified.
    /// </summary>
    /// <example>
    /// var redacted = DataAvailability.Redacted&lt;string&gt;();
    /// </example>
    public static DataWithAvailability<T> Redacted<T>()
        => new(default, DataAvailabilityStatus.Redacted);

    /// <summary>
    /// Creates a low quality value. Type is inferred from the value.
    /// </summary>
    /// <example>
    /// var lowQuality = DataAvailability.Low("Unreliable data");
    /// </example>
    public static DataWithAvailability<T> Low<T>(T value)
        => new(value, DataAvailabilityStatus.Low);

    public static DataWithAvailability<int> FromIntegerString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return NotAvailable<int>();
        }

        return int.TryParse(value, out var intValue)
            ? Available(intValue)
            : NotAvailable<int>();
    }

    public static DataWithAvailability<decimal> FromDecimalString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return NotAvailable<decimal>();
        }

        return decimal.TryParse(value, out var decimalValue)
            ? Available(decimalValue)
            : NotAvailable<decimal>();
    }

    public static DataWithAvailability<T> FromNullable<T>(Nullable<T> value) where T : struct
    {
        return value.HasValue
            ? Available(value.Value)
            : NotAvailable<T>();
    }

    /// <summary>
    /// Maps a string value to DataWithAvailability, handling GIAS special codes.
    /// </summary>
    public static DataWithAvailability<string> FromStringWithCodes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return NotAvailable<string>();
        }

        if (EesDataCodes.IsRedacted(value))
        {
            return Redacted<string>();
        }

        if (EesDataCodes.IsNotApplicable(value))
        {
            return NotApplicable<string>();
        }

        if (EesDataCodes.IsNotAvailable(value))
        {
            return NotAvailable<string>();
        }

        return Available(value);
    }

    /// <summary>
    /// Maps a string value that cannot have special codes.
    /// </summary>
    public static DataWithAvailability<string> FromStringWithoutCodes(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? NotAvailable<string>()
            : Available(value);
    }

}