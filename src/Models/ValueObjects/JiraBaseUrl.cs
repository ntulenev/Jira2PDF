namespace JiraReport.Models.ValueObjects;

internal readonly record struct JiraBaseUrl
{
    public JiraBaseUrl(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var parsed))
        {
            throw new ArgumentException("Base URL must be a valid absolute URI.", nameof(value));
        }

        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parsed.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Base URL must start with http:// or https://.", nameof(value));
        }

        Value = parsed.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    public string Value { get; }

    public override string ToString() => Value;
}
