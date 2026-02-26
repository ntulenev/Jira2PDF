namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents user-visible error message text.
/// </summary>
internal readonly record struct ErrorMessage
{
    /// <summary>
    /// Initializes a new <see cref="ErrorMessage"/> instance.
    /// </summary>
    /// <param name="value">Raw error message text.</param>
    public ErrorMessage(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Builds error message from exception.
    /// </summary>
    /// <param name="exception">Source exception.</param>
    /// <returns>Error message value object.</returns>
    public static ErrorMessage FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ErrorMessage(string.IsNullOrWhiteSpace(exception.Message)
            ? "Unknown error."
            : exception.Message);
    }

    /// <summary>
    /// Gets normalized error text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns error text representation.
    /// </summary>
    /// <returns>Error message text.</returns>
    public override string ToString() => Value;
}
