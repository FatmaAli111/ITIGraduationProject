using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

public class AILayerClient : IAILayerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _flowId;

    public AILayerClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _flowId = config["AILayer:FlowId"]!;
    }

    public async Task<string> GenerateTemplateImageAsync(
        AIGenerateRequest request, CancellationToken ct = default)
    {
        var message = $"""
            Product: {request.ProductName}
            Category: {request.CategoryName}
            Style: {request.StyleType}
            Colors: {request.FavoriteColors}
            Interests: {request.Interests}
            Design Preference: {request.DesignPreference}
            """;

        var payload = new
        {
            input_value = message,
            output_type = "chat",
            input_type = "chat"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/run/{_flowId}?stream=false", payload, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Langflow wraps the result: outputs[0].outputs[0].results.message.data.data.url
        var imageUrl = json
            .GetProperty("outputs")[0]
            .GetProperty("outputs")[0]
            .GetProperty("results")
            .GetProperty("message")
            .GetProperty("data")
            .GetProperty("data")
            .GetProperty("url")
            .GetString();

        return imageUrl ?? throw new Exception("AI layer returned no image URL");
    }
}