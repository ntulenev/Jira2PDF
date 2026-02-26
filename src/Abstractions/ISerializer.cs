namespace JiraReport.Abstractions;

internal interface ISerializer
{
    T? Deserialize<T>(string json);
}
