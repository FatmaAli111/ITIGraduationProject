using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using ITIGraduationProject.Service.ReportGenerator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace ITIGraduationProject.Tests.Services;

[TestFixture]
public class LangflowServiceTests
{
    [Test]
    public async Task SendMessageAsync_ForwardsAdminAuthorizationAndReturnsReportText()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            requestBody = await request.Content!.ReadAsStringAsync();
            request.Headers.GetValues("x-api-key").Single().Should().Be("langflow-key");
            request.RequestUri!.AbsolutePath.Should().Be("/api/v1/run/report-flow");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"outputs":[{"outputs":[{"results":{"message":{"text":"Generated report"}}}]}]}""",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var service = CreateService(handler, "Bearer admin-token");

        var result = await service.SendMessageAsync("Revenue report", "session-1");

        result.Should().Be("Generated report");
        using var document = JsonDocument.Parse(requestBody!);
        var headers = document.RootElement
            .GetProperty("tweaks")
            .GetProperty("analytics-node")
            .GetProperty("headers");
        headers.EnumerateArray()
            .Should().Contain(header =>
                header.GetProperty("key").GetString() == "Authorization" &&
                header.GetProperty("value").GetString() == "Bearer admin-token");
    }

    [Test]
    public async Task SendMessageAsync_RejectsRequestWithoutAdminAuthorization()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var service = CreateService(handler, null);

        var action = () => service.SendMessageAsync("Revenue report", "session-1");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*authorization token*");
    }

    private static LangflowService CreateService(
        HttpMessageHandler handler,
        string? authorization)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:7860/")
        };
        var settings = Options.Create(new ReportGeneratorSettings
        {
            BaseUrl = "http://localhost:7860/",
            FlowId = "report-flow",
            ApiKey = "langflow-key",
            AnalyticsToolComponentId = "analytics-node"
        });
        var context = new DefaultHttpContext();
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        return new LangflowService(
            httpClient,
            settings,
            new HttpContextAccessor { HttpContext = context },
            NullLogger<LangflowService>.Instance);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => handler(request);
    }
}
