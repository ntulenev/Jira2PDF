using System.Text.Json;

using JiraReport.Abstractions;

namespace JiraReport.Transport;

/// <summary>
/// System.Text.Json serializer implementation.
/// </summary>
internal sealed class SimpleJsonSerializer : ISerializer
{
    /// <inheritdoc />
    public T? Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
