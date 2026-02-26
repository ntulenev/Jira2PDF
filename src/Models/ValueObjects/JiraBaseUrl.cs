namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated Jira base URL value.
/// </summary>
internal readonly record struct JiraBaseUrl
{
    /// <summary>
    /// Initializes a new <see cref="JiraBaseUrl"/> instance.
    /// </summary>
    /// <param name="value">Raw base URL string.</param>
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

    /// <summary>
    /// Gets normalized base URL text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns URL text representation.
    /// </summary>
    /// <returns>Normalized URL text.</returns>
    public override string ToString() => Value;
}
