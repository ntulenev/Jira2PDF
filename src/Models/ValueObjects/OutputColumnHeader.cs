namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated output column header value.
/// </summary>
internal readonly record struct OutputColumnHeader
{
    /// <summary>
    /// Initializes a new <see cref="OutputColumnHeader"/> instance.
    /// </summary>
    /// <param name="value">Raw output column header value.</param>
    public OutputColumnHeader(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized output column header text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Builds header value from issue field key.
    /// </summary>
    /// <param name="fieldKey">Issue field key.</param>
    /// <returns>Normalized output column header.</returns>
    public static OutputColumnHeader FromFieldKey(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return new OutputColumnHeader("Field");
        }

        var words = fieldKey
            .Trim()
            .Replace('_', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return new OutputColumnHeader("Field");
        }

        var headerWords = new List<string>(words.Length);
        foreach (var word in words)
        {
            var characters = word.ToCharArray();
            characters[0] = char.ToUpperInvariant(characters[0]);
            headerWords.Add(new string(characters));
        }

        return new OutputColumnHeader(string.Join(' ', headerWords));
    }

    /// <summary>
    /// Returns output column header text representation.
    /// </summary>
    /// <returns>Output column header text.</returns>
    public override string ToString() => Value;
}
