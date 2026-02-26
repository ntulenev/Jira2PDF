namespace JiraReport.Models.ValueObjects;

internal readonly record struct ErrorMessage
{
    public ErrorMessage(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public static ErrorMessage FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ErrorMessage(string.IsNullOrWhiteSpace(exception.Message)
            ? "Unknown error."
            : exception.Message);
    }

    public string Value { get; }

    public override string ToString() => Value;
}
