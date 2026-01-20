namespace SAPSec.Core.Model;

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
public static class DataAvailability
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
}