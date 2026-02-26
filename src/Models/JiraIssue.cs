namespace JiraReport.Models;

/// <summary>
/// Represents issue data used in report output.
/// </summary>
/// <param name="Key">Issue key.</param>
/// <param name="Fields">Normalized field values by key.</param>
internal sealed record JiraIssue(
    string Key,
    IReadOnlyDictionary<string, string> Fields)
{
    /// <summary>
    /// Gets normalized field value by key.
    /// </summary>
    /// <param name="fieldKey">Field key.</param>
    /// <returns>Field value or dash if missing.</returns>
    public string GetFieldValue(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return "-";
        }

        if (string.Equals(fieldKey, "key", StringComparison.OrdinalIgnoreCase))
        {
            return Key;
        }

        return Fields.TryGetValue(fieldKey, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : "-";
    }
}
