using System.Text;
using System.Text.Json;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITIGraduationProject.Service.ReportGenerator;

public class LangflowService : ILangflowService
{
    private readonly HttpClient _httpClient;
    private readonly ReportGeneratorSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LangflowService> _logger;

    public LangflowService(
        HttpClient httpClient,
        IOptions<ReportGeneratorSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LangflowService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(
        string message,
        string sessionId)
    {
        ValidateConfiguration();

        var authorization = _httpContextAccessor.HttpContext?
            .Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization))
        {
            throw new InvalidOperationException(
                "The report request is missing the signed-in admin authorization token.");
        }

        var request = new
        {
            output_type = "chat",
            input_type = "chat",
            input_value = message,
            session_id = sessionId,
            tweaks = new Dictionary<string, object>
            {
                [_settings.AnalyticsToolComponentId] = new
                {
                    headers = new[]
                    {
                        new { key = "User-Agent", value = "Langflow/1.0" },
                        new { key = "Accept", value = "application/json" },
                        new { key = "Authorization", value = authorization }
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/v1/run/{Uri.EscapeDataString(_settings.FlowId)}")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json")
        };
        httpRequest.Headers.Add("x-api-key", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Langflow report request failed with status code {StatusCode}",
                (int)response.StatusCode);
            throw new HttpRequestException(
                $"Langflow report request failed with status code {(int)response.StatusCode}: {responseBody}");
        }

        return ExtractResponseText(responseBody);
    }

    private void ValidateConfiguration()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                "ReportGenerator:BaseUrl is missing or invalid.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FlowId))
        {
            throw new InvalidOperationException(
                "ReportGenerator:FlowId is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "ReportGenerator:ApiKey is not configured. Set it with user secrets or an environment variable.");
        }

        if (string.IsNullOrWhiteSpace(_settings.AnalyticsToolComponentId))
        {
            throw new InvalidOperationException(
                "ReportGenerator:AnalyticsToolComponentId is not configured.");
        }
    }

    private static string ExtractResponseText(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);

        try
        {
            var text = document.RootElement
                .GetProperty("outputs")[0]
                .GetProperty("outputs")[0]
                .GetProperty("results")
                .GetProperty("message")
                .GetProperty("text")
                .GetString();

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }
        catch (KeyNotFoundException)
        {
            // Return the stable error below if Langflow changes its response shape.
        }
        catch (InvalidOperationException)
        {
            // Return the stable error below if Langflow changes its response shape.
        }

        throw new InvalidOperationException(
            "Langflow returned a successful response without report text.");
    }
}
