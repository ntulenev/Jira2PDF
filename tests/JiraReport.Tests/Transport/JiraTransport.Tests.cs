using System.Net;
using System.Text;

using FluentAssertions;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;
using JiraReport.Transport;
using JiraReport.Transport.Models;

using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

namespace JiraReport.Tests.Transport;

public sealed class JiraTransportTests
{
    [Fact(DisplayName = "Constructor throws when http client is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenHttpClientIsNullThrowsArgumentNullException()
    {
        // Arrange
        HttpClient http = null!;
        var retryPolicy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 0)));
        var serializer = new SimpleJsonSerializer();

        // Act
        Action act = () => _ = new JiraTransport(http, retryPolicy, serializer);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when retry policy is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenRetryPolicyIsNullThrowsArgumentNullException()
    {
        // Arrange
        using var http = new HttpClient();
        IJiraRetryPolicy retryPolicy = null!;
        var serializer = new SimpleJsonSerializer();

        // Act
        Action act = () => _ = new JiraTransport(http, retryPolicy, serializer);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when serializer is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenSerializerIsNullThrowsArgumentNullException()
    {
        // Arrange
        using var http = new HttpClient();
        var retryPolicy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 0)));
        ISerializer serializer = null!;

        // Act
        Action act = () => _ = new JiraTransport(http, retryPolicy, serializer);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "GetAsync returns deserialized DTO when response is valid")]
    [Trait("Category", "Unit")]
    public async Task GetAsyncWhenResponseIsValidReturnsDto()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var sendCalls = 0;
        var baseUri = new Uri("https://example.test/");
        var requestUrl = new Uri(baseUri, "issue/APP-1");

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"Bug\"}", Encoding.UTF8, "application/json")
        };

        var handler = CreateHandler(
            requestUrl,
            () =>
            {
                sendCalls++;
                return response;
            },
            cts.Token);

        using var http = new HttpClient(handler.Object) { BaseAddress = baseUri };
        var transport = new JiraTransport(http, new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 0))), new SimpleJsonSerializer());

        // Act
        var result = await transport.GetAsync<JiraNamedEntityResponse>(new Uri("issue/APP-1", UriKind.Relative), cts.Token);

        // Assert
        sendCalls.Should().Be(1);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bug");
    }

    [Fact(DisplayName = "GetAsync returns null when response body is null")]
    [Trait("Category", "Unit")]
    public async Task GetAsyncWhenResponseBodyIsNullReturnsNull()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var requestUrl = new Uri("https://example.test/issue/APP-1");

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        var handler = CreateHandler(requestUrl, () => response, cts.Token);
        using var http = new HttpClient(handler.Object);
        var transport = new JiraTransport(http, new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 0))), new SimpleJsonSerializer());

        // Act
        var result = await transport.GetAsync<JiraNamedEntityResponse>(requestUrl, cts.Token);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetAsync throws when response is not successful")]
    [Trait("Category", "Unit")]
    public async Task GetAsyncWhenResponseIsFailureThrowsHttpRequestException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var requestUrl = new Uri("https://example.test/issue/APP-1");

        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request",
            Content = new StringContent("invalid query", Encoding.UTF8, "text/plain")
        };

        var handler = CreateHandler(requestUrl, () => response, cts.Token);
        using var http = new HttpClient(handler.Object);
        var transport = new JiraTransport(http, new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 0))), new SimpleJsonSerializer());

        // Act
        Func<Task> act = () => transport.GetAsync<JiraNamedEntityResponse>(requestUrl, cts.Token);

        // Assert
        await act.Should()
            .ThrowAsync<HttpRequestException>();
    }

    [Fact(DisplayName = "GetAsync retries transient failure and succeeds")]
    [Trait("Category", "Unit")]
    public async Task GetAsyncWhenTransientFailureRetriesAndSucceeds()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var sendCalls = 0;
        var requestUrl = new Uri("https://example.test/issue/APP-1");

        using var firstResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            ReasonPhrase = "Service Unavailable",
            Content = new StringContent("temporary", Encoding.UTF8, "text/plain")
        };

        using var secondResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"Bug\"}", Encoding.UTF8, "application/json")
        };

        var responses = new Queue<HttpResponseMessage>([firstResponse, secondResponse]);
        var handler = CreateHandler(
            requestUrl,
            () =>
            {
                sendCalls++;
                return responses.Dequeue();
            },
            cts.Token);

        using var http = new HttpClient(handler.Object);
        var transport = new JiraTransport(http, new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 1))), new SimpleJsonSerializer());

        // Act
        var result = await transport.GetAsync<JiraNamedEntityResponse>(requestUrl, cts.Token);

        // Assert
        sendCalls.Should().Be(2);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bug");
    }

    private static Mock<HttpMessageHandler> CreateHandler(
        Uri requestUrl,
        Func<HttpResponseMessage> responseFactory,
        CancellationToken cancellationToken)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == requestUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => responseFactory());

        return handler;
    }

    private static AppSettings CreateSettings(int retryCount)
    {
        return new AppSettings(
            new JiraBaseUrl("https://example.test"),
            new JiraEmail("user@example.test"),
            new JiraApiToken("token"),
            50,
            retryCount,
            [new ReportConfig(new ReportName("Backlog"), new JqlQuery("project = APP"), [], [], new PdfReportName("Sprint report"))]);
    }
}
