namespace JiraReport.Abstractions;

/// <summary>
/// Defines JSON deserialization abstraction.
/// </summary>
internal interface ISerializer
{
    /// <summary>Serializes a value to JSON.</summary>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes JSON payload into target type.
    /// </summary>
    /// <typeparam name="T">Target model type.</typeparam>
    /// <param name="json">JSON payload.</param>
    /// <returns>Deserialized model instance.</returns>
    T? Deserialize<T>(string json);
}
