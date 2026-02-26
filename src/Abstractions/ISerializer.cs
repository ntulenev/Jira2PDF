namespace JiraReport.Abstractions;

/// <summary>
/// Defines JSON deserialization abstraction.
/// </summary>
internal interface ISerializer
{
    /// <summary>
    /// Deserializes JSON payload into target type.
    /// </summary>
    /// <typeparam name="T">Target model type.</typeparam>
    /// <param name="json">JSON payload.</param>
    /// <returns>Deserialized model instance.</returns>
    T? Deserialize<T>(string json);
}
