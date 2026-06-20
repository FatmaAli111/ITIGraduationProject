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
        var raw = await response.Content.ReadAsStringAsync(ct);
        var root = JsonDocument.Parse(raw).RootElement;


        // Step 1: navigate to the actual response shape
        var innerText = root
            .GetProperty("outputs")[0]
            .GetProperty("outputs")[0]
            .GetProperty("results")
            .GetProperty("message")
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(innerText))
            throw new Exception("AI layer returned empty response");

        // Step 2: strip the ```json ... ``` markdown fence
        var cleaned = innerText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        // Step 3: parse the inner JSON to get the actual image URL
        var innerJson = JsonDocument.Parse(cleaned).RootElement;
        var imageUrl = innerJson.GetProperty("url").GetString();

        return imageUrl ?? throw new Exception("AI layer returned no image URL");
    }
}