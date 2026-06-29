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
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;

        if (!root.TryGetProperty("outputs", out var outerOutputs) ||
            outerOutputs.ValueKind != JsonValueKind.Array ||
            outerOutputs.GetArrayLength() == 0 ||
            !outerOutputs[0].TryGetProperty("outputs", out var innerOutputs) ||
            innerOutputs.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("AI layer response did not contain an outputs array.");
        }

        foreach (var output in innerOutputs.EnumerateArray())
        {
            if (!output.TryGetProperty("results", out var results) ||
                !results.TryGetProperty("message", out var messageResult) ||
                !messageResult.TryGetProperty("text", out var textResult))
            {
                continue;
            }

            var text = textResult.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var cleaned = text
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();

            try
            {
                using var innerDocument = JsonDocument.Parse(cleaned);
                if (innerDocument.RootElement.TryGetProperty("url", out var urlProperty))
                {
                    var imageUrl = urlProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        return imageUrl;
                    }
                }
            }
            catch (JsonException)
            {
                // This output is narrative text rather than the image result.
            }
        }

        throw new InvalidOperationException("AI layer returned no image URL.");
    }
}
