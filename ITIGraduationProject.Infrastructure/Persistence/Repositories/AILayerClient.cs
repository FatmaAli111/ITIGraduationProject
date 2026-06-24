using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ITIGraduationProject.Application.Interfaces.IRepositories;

public class AILayerClient : IAILayerClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _mistralClient;
    private readonly HttpClient _jigsawClient;
    private readonly string _flowId;
    private readonly string _mistralApiKey;
    private readonly string _jigsawApiKey;
    private readonly string _langFlowBaseUrl;
    private readonly bool _useDirectApi;

    private static readonly string[] VariationNotes = new[]
    {
        "Variation note: this is variation 1 of 3. Lean into clean line-art and a single bold accent color.",
        "Variation note: this is variation 2 of 3. Lean into a grungier, distressed texture and a different accent color than variation 1.",
        "Variation note: this is variation 3 of 3. Lean into bold typography or lettering combined with a graphic motif, distinct from variations 1 and 2.",
    };

    private static readonly string BasePrompt = @"
You are an AI fashion template generator.

You generate AI image prompts for printable streetwear t-shirt graphics.

You MUST follow these steps internally:
1. Read the Stored Preference first - it defines mandatory design DNA such as color palette, mood, theme, material, or motif that the final design must visibly express.
2. Merge the Stored Preference with the Current Request into one unified design direction, treating the preference as a core constraint rather than optional inspiration.
3. If the Current Request conflicts with the Stored Preference, the Stored Preference always wins.
4. Generate a strong streetwear design concept where the merged preference is clearly visible in the final image_prompt (through color, motif, material, or theme), not just implied.

Requirements:
- print-ready
- transparent background
- graphic only
- no t-shirt mockup
- streetwear aesthetic
- the design must visibly reflect the Stored Preference

Rules:
- concept_name must be 1-4 words, creative, brandable
- image_prompt must be ONE sentence only
- image_prompt must explicitly mention the preference's defining element (color, motif, or theme)
- Do NOT explain anything
- Do NOT use markdown";

    public AILayerClient(
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _httpClient = httpClientFactory.CreateClient("LangFlow");
        _mistralClient = httpClientFactory.CreateClient("Mistral");
        _jigsawClient = httpClientFactory.CreateClient("JigsawStack");

        _flowId = config["AILayer:FlowId"] ?? "";
        _langFlowBaseUrl = config["AILayer:BaseUrl"] ?? "http://localhost:7860";
        _mistralApiKey = config["AI:Direct:MistralApiKey"] ?? "";
        _jigsawApiKey = config["AI:Direct:JigsawStackApiKey"] ?? "";
        _useDirectApi = bool.TryParse(config["AI:Direct:UseDirectApi"], out var val) && val;
    }

    // ────────────── Original LangFlow method (kept for backward compat) ──────────────

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

        var innerText = root
            .GetProperty("outputs")[0]
            .GetProperty("outputs")[0]
            .GetProperty("results")
            .GetProperty("message")
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(innerText))
            throw new Exception("AI layer returned empty response");

        var cleaned = innerText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var innerJson = JsonDocument.Parse(cleaned).RootElement;
        var imageUrl = innerJson.GetProperty("url").GetString();

        return imageUrl ?? throw new Exception("AI layer returned no image URL");
    }

    // ────────────── New 3-Variations method ──────────────

    public async Task<List<GeneratedVariationResult>> GenerateThreeVariationsAsync(
        AIGenerateRequest request, string userMessage, CancellationToken ct = default)
    {
        if (_useDirectApi)
            return await GenerateDirectAsync(request, userMessage, ct);

        // Fallback: try LangFlow (call 3 times)
        return await GenerateViaLangFlowAsync(request, userMessage, ct);
    }

    // ────────────── Direct API: Mistral + JigsawStack ──────────────

    private async Task<List<GeneratedVariationResult>> GenerateDirectAsync(
        AIGenerateRequest request, string userMessage, CancellationToken ct)
    {
        var preferenceJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            favorite_colors = request.FavoriteColors,
            fashion_style = request.StyleType,
            interests = request.Interests,
            design_preference = request.DesignPreference,
        });

        var tasks = new Task<GeneratedVariationResult>[3];

        for (int i = 0; i < 3; i++)
        {
            int idx = i; // capture
            tasks[i] = Task.Run(async () =>
            {
                // Step 1: Generate image prompt via Mistral
                var prompt = BuildPrompt(preferenceJson, userMessage, VariationNotes[idx]);

                var mistralPayload = new
                {
                    model = "mistral-large-latest",
                    messages = new object[]
                    {
                        new { role = "assistant", content = "You are a fashion design AI. Always respond with valid JSON only." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 300
                };

                var mistralResponse = await _mistralClient.PostAsJsonAsync(
                    "/v1/chat/completions", mistralPayload, ct);
                mistralResponse.EnsureSuccessStatusCode();

                var mistralRaw = await mistralResponse.Content.ReadAsStringAsync(ct);
                var mistralRoot = JsonDocument.Parse(mistralRaw).RootElement;
                var content = mistralRoot
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                // Parse JSON from response
                var parsed = ParseConceptAndPrompt(content);

                // Step 2: Generate image via JigsawStack
                var jigsawPayload = new
                {
                    prompt = parsed.ImagePrompt,
                    n = 1,
                    size = "1024x1024"
                };

                var jigsawResponse = await _jigsawClient.PostAsJsonAsync(
                    "/v1/image/generations", jigsawPayload, ct);
                jigsawResponse.EnsureSuccessStatusCode();

                var jigsawRaw = await jigsawResponse.Content.ReadAsStringAsync(ct);
                var jigsawRoot = JsonDocument.Parse(jigsawRaw).RootElement;
                var imageUrl = jigsawRoot
                    .GetProperty("data")[0]
                    .GetProperty("url")
                    .GetString() ?? "";

                return new GeneratedVariationResult
                {
                    ConceptName = parsed.ConceptName,
                    ImagePrompt = parsed.ImagePrompt,
                    ImageUrl = imageUrl,
                };
            }, ct);
        }

        await Task.WhenAll(tasks);

        return tasks.Select(t => t.Result).ToList();
    }

    // ────────────── LangFlow Fallback: call the flow 3 times ──────────────

    private async Task<List<GeneratedVariationResult>> GenerateViaLangFlowAsync(
        AIGenerateRequest request, string userMessage, CancellationToken ct)
    {
        var results = new List<GeneratedVariationResult>();

        for (int i = 0; i < 3; i++)
        {
            var combinedMessage = $"{userMessage}\n\n{VariationNotes[i]}";
            var imageUrl = await CallLangFlowWithMessageAsync(request, combinedMessage, ct);

            results.Add(new GeneratedVariationResult
            {
                ConceptName = $"Variation {i + 1}",
                ImagePrompt = combinedMessage,
                ImageUrl = imageUrl,
            });
        }

        return results;
    }

    private async Task<string> CallLangFlowWithMessageAsync(
        AIGenerateRequest request, string message, CancellationToken ct)
    {
        var fullMessage = $"""
            Product: {request.ProductName}
            Category: {request.CategoryName}
            Style: {request.StyleType}
            Colors: {request.FavoriteColors}
            Interests: {request.Interests}
            Design Preference: {request.DesignPreference}

            {message}
            """;

        var payload = new
        {
            input_value = fullMessage,
            output_type = "chat",
            input_type = "chat"
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/run/{_flowId}?stream=false", payload, ct);

        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync(ct);
        var root = JsonDocument.Parse(raw).RootElement;

        var innerText = root
            .GetProperty("outputs")[0]
            .GetProperty("outputs")[0]
            .GetProperty("results")
            .GetProperty("message")
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(innerText))
            throw new Exception("AI layer returned empty response");

        var cleaned = innerText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var innerJson = JsonDocument.Parse(cleaned).RootElement;
        return innerJson.GetProperty("url").GetString()
            ?? throw new Exception("AI layer returned no image URL");
    }

    // ────────────── Helpers ──────────────

    private static string BuildPrompt(string preferenceJson, string userMessage, string variationNote)
    {
        return $@"{BasePrompt}

Stored Preference:
{preferenceJson}

Current Request:
{userMessage}

{variationNote}

You MUST respond in this exact JSON format only, nothing else:
{{""concept_name"": ""..."", ""image_prompt"": ""...""}}";
    }

    private static (string ConceptName, string ImagePrompt) ParseConceptAndPrompt(string content)
    {
        try
        {
            var doc = JsonDocument.Parse(content).RootElement;
            return (
                doc.GetProperty("concept_name").GetString() ?? "Untitled",
                doc.GetProperty("image_prompt").GetString() ?? ""
            );
        }
        catch
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*?\}");
            if (match.Success)
            {
                var doc = JsonDocument.Parse(match.Value).RootElement;
                return (
                    doc.GetProperty("concept_name").GetString() ?? "Untitled",
                    doc.GetProperty("image_prompt").GetString() ?? ""
                );
            }
            return ("Untitled", content);
        }
    }
}