namespace JiraReport.Models.ValueObjects;

internal readonly record struct JiraApiToken
{
    public JiraApiToken(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
