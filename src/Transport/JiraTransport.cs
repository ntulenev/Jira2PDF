using JiraReport.Abstractions;

namespace JiraReport.Transport;

internal sealed class JiraTransport : IJiraTransport
{
    public JiraTransport(HttpClient http, IJiraRetryPolicy retryPolicy, ISerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(retryPolicy);
        ArgumentNullException.ThrowIfNull(serializer);

        _http = http;
        _retryPolicy = retryPolicy;
        _serializer = serializer;
    }

    public async Task<TDto?> GetAsync<TDto>(Uri url, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(url);

        var attempt = 0;

        while (true)
        {
            try
            {
                using var response = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    return _serializer.Deserialize<TDto>(json);
                }

                if (_retryPolicy.TryGetDelay(attempt + 1, response.StatusCode, null, out var retryDelay))
                {
                    attempt++;
                    await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new HttpRequestException(
                    $"Jira API error {(int)response.StatusCode} {response.ReasonPhrase}. Url={url}. Body={body}",
                    null,
                    response.StatusCode);
            }
            catch (HttpRequestException ex) when (_retryPolicy.TryGetDelay(attempt + 1, ex.StatusCode, ex, out var retryDelay))
            {
                attempt++;
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private readonly HttpClient _http;
    private readonly IJiraRetryPolicy _retryPolicy;
    private readonly ISerializer _serializer;
}
