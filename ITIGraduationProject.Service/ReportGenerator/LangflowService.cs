using System.Text;
using System.Text.Json;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;

namespace ITIGraduationProject.Service.ReportGenerator
{
    public class LangflowService : ILangflowService
    {
        private readonly HttpClient _httpClient;

    private const string FlowId =
        "bc27f2c7-a799-4eff-a8ae-78746f3a9e9d";

        private const string ApiKey =
            "sk-z7uGpwGhR0exUkKnJhjVEs47JiSJcxxYJkQhvD8eQgA";

        public LangflowService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "x-api-key",
                    ApiKey);
            }
        }

        public async Task<string> SendMessageAsync(
            string message,
            string sessionId)
        {
            var request = new
            {
                output_type = "chat",
                input_type = "chat",
                input_value = message,
                session_id = sessionId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"api/v1/run/{FlowId}",
                content);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Langflow Error ({(int)response.StatusCode}): {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);

            return doc.RootElement
                .GetProperty("outputs")[0]
                .GetProperty("outputs")[0]
                .GetProperty("results")
                .GetProperty("message")
                .GetProperty("text")
                .GetString()
                ?? "No Response";
        }
    }

}
