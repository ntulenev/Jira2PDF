using System.Text.Json;

using JiraReport.Abstractions;

namespace JiraReport.Transport;

internal sealed class SimpleJsonSerializer : ISerializer
{
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
