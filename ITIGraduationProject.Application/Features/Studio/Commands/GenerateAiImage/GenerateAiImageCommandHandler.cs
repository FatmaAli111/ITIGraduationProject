using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ITIGraduationProject.Application.Features.Studio.Commands.GenerateAiImage
{
    public class GenerateAiImageCommandHandler : IRequestHandler<GenerateAiImageCommand, GenerateAiImageResult>
    {
        // -----------------------------------------------------------------------
        // The LangFlow flow ID for AI image generation (Design Studio).
        // This is the fixed flow ID provided in the task specification.
        // -----------------------------------------------------------------------
        private const string DefaultImageFlowId = "bb7fdffd-50aa-4d89-8942-a6faaac93c31";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GenerateAiImageCommandHandler> _logger;

        public GenerateAiImageCommandHandler(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<GenerateAiImageCommandHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GenerateAiImageResult> Handle(
            GenerateAiImageCommand request,
            CancellationToken cancellationToken)
        {
            // ---------------------------------------------------------------
            // 1. Load configuration — API key must never be hardcoded.
            // ---------------------------------------------------------------
            var apiKey = _configuration["AILayer:ApiKey"]
                ?? throw new InvalidOperationException("LangFlow API key is not configured (AILayer:ApiKey).");

            var langFlowBaseUrl = _configuration["AILayer:BaseUrl"]
                ?? "http://localhost:7860";

            var userId = _currentUserService.UserId;

            // ---------------------------------------------------------------
            // 2. Call LangFlow with the exact request format specified.
            //    A new Guid session_id is generated for every request.
            // ---------------------------------------------------------------
            var sessionId = Guid.NewGuid().ToString();
            var flowId = _configuration["AILayer:ImageFlowId"] ?? DefaultImageFlowId;
            var flowEndpoint = $"{langFlowBaseUrl.TrimEnd('/')}/api/v1/run/{flowId}";

            _logger.LogInformation(
                "[GenerateAiImage] Calling LangFlow at {Endpoint} with session {SessionId}",
                flowEndpoint, sessionId);

            var langFlowPayload = new
            {
                output_type = "chat",
                input_type = "chat",
                input_value = request.Prompt,
                session_id = sessionId
            };

            using var langFlowClient = _httpClientFactory.CreateClient();
            langFlowClient.Timeout = TimeSpan.FromSeconds(300);

            using var langFlowRequest = new HttpRequestMessage(HttpMethod.Post, flowEndpoint);
            langFlowRequest.Headers.Add("x-api-key", apiKey);
            langFlowRequest.Content = new StringContent(
                JsonSerializer.Serialize(langFlowPayload),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage langFlowResponse;
            try
            {
                langFlowResponse = await langFlowClient.SendAsync(langFlowRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GenerateAiImage] LangFlow is unreachable at {Endpoint}", flowEndpoint);
                throw new InvalidOperationException("LangFlow service is currently unavailable. Please try again later.", ex);
            }

            if (!langFlowResponse.IsSuccessStatusCode)
            {
                var errorBody = await langFlowResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "[GenerateAiImage] LangFlow returned {StatusCode}: {Body}",
                    (int)langFlowResponse.StatusCode, errorBody);

                throw new InvalidOperationException(
                    $"LangFlow returned an error ({(int)langFlowResponse.StatusCode}). Check server logs for details.");
            }

            var rawResponse = await langFlowResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("[GenerateAiImage] LangFlow raw response received (length={Len})", rawResponse.Length);

            // ---------------------------------------------------------------
            // 3. Parse the LangFlow response.
            //    The response contains multiple "outputs" entries. The image URL
            //    is embedded inside a JSON code block (```json ... ```) in the
            //    text of the second Chat Output component.
            //    Extract the first valid, unique URL only.
            // ---------------------------------------------------------------
            string imageUrl = ExtractFirstImageUrl(rawResponse);

            _logger.LogInformation("[GenerateAiImage] Extracted image URL: {Url}", imageUrl);

            // ---------------------------------------------------------------
            // 4. Download the image as raw binary using HttpClient.
            //    Determine the real file extension from Content-Type, not from URL.
            // ---------------------------------------------------------------
            using var downloadClient = _httpClientFactory.CreateClient();
            downloadClient.Timeout = TimeSpan.FromSeconds(120);

            _logger.LogInformation("[GenerateAiImage] Downloading image from {Url}", imageUrl);

            HttpResponseMessage downloadResponse;
            try
            {
                downloadResponse = await downloadClient.GetAsync(imageUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GenerateAiImage] Failed to download image from {Url}", imageUrl);
                throw new InvalidOperationException("Failed to download the generated image from the AI service.", ex);
            }

            _logger.LogInformation(
                "[GenerateAiImage] Download response — StatusCode: {Status} | Content-Type: {ContentType} | Content-Length: {Length}",
                (int)downloadResponse.StatusCode,
                downloadResponse.Content.Headers.ContentType?.MediaType ?? "unknown",
                downloadResponse.Content.Headers.ContentLength?.ToString() ?? "unknown");

            if (!downloadResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Image download failed with status code {(int)downloadResponse.StatusCode}.");
            }

            var contentType = downloadResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "[GenerateAiImage] Invalid content type received: '{ContentType}'. Expected image/*.",
                    contentType);
                throw new InvalidOperationException(
                    $"The AI service returned a non-image response (Content-Type: '{contentType}'). Cannot save as image.");
            }

            // Determine extension from actual Content-Type — never from URL.
            var extension = GetExtensionFromContentType(contentType);

            // Read raw binary content — never convert to string or Base64.
            var imageBytes = await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);

            if (imageBytes.Length == 0)
            {
                throw new InvalidOperationException("The downloaded image has zero bytes and cannot be saved.");
            }

            _logger.LogInformation(
                "[GenerateAiImage] Image downloaded successfully — ContentType: {ContentType}, Extension: {Ext}, Size: {Size} bytes",
                contentType, extension, imageBytes.Length);

            // ---------------------------------------------------------------
            // 5. Save raw bytes to wwwroot/GraphicAssets/{UserId}/{guid}.{ext}
            // ---------------------------------------------------------------
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            }

            var userFolder = Path.Combine(webRootPath, "GraphicAssets", userId.ToString());
            Directory.CreateDirectory(userFolder); // Creates all intermediate directories if missing.

            var fileNameWithoutExt = Guid.NewGuid().ToString("N")[..8]; // e.g. "7c2f3e11"
            var fileName = $"{fileNameWithoutExt}.{extension}";
            var filePath = Path.Combine(userFolder, fileName);

            // Write raw bytes directly — no encoding conversion.
            await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

            // Verify the file exists and has content.
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                throw new InvalidOperationException($"Image file was not saved correctly at '{filePath}'.");
            }

            _logger.LogInformation(
                "[GenerateAiImage] Image saved to '{FilePath}' ({Size} bytes)",
                filePath, fileInfo.Length);

            // Local URL that the Angular client will use to display the image.
            var localImageUrl = $"/GraphicAssets/{userId}/{fileName}";

            // ---------------------------------------------------------------
            // 6. Persist a GraphicAsset record linked to the authenticated user.
            //    Reuse the existing ImageUrl property — no new fields created.
            // ---------------------------------------------------------------
            var asset = new GraphicAsset
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"AI Generated – {request.Prompt.Truncate(80)}",
                Type = GraphicAssetType.AIGenerated,
                ImageUrl = localImageUrl,
                Tags = "ai-generated"
            };

            await _unitOfWork.GraphicAssets.AddAsync(asset);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "[GenerateAiImage] GraphicAsset {AssetId} created for user {UserId} with ImageUrl '{ImageUrl}'",
                asset.Id, userId, localImageUrl);

            // ---------------------------------------------------------------
            // 7. Return the asset ID and local image URL.
            //    Never return the external LangFlow URL.
            // ---------------------------------------------------------------
            return new GenerateAiImageResult(asset.Id, localImageUrl);
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        /// <summary>
        /// Parses the LangFlow response JSON and returns the first valid, unique image URL.
        /// The URL is embedded inside a JSON code block (```json ... ```) in the message text
        /// of one of the Chat Output components.
        /// </summary>
        private string ExtractFirstImageUrl(string rawResponse)
        {
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(rawResponse);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "[GenerateAiImage] LangFlow response is not valid JSON.");
                throw new InvalidOperationException("LangFlow returned an invalid JSON response.", ex);
            }

            using (doc)
            {
                var root = doc.RootElement;

                if (!root.TryGetProperty("outputs", out var outerOutputs) ||
                    outerOutputs.ValueKind != JsonValueKind.Array ||
                    outerOutputs.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("LangFlow response missing 'outputs' array.");
                }

                var firstOuter = outerOutputs[0];

                if (!firstOuter.TryGetProperty("outputs", out var innerOutputs) ||
                    innerOutputs.ValueKind != JsonValueKind.Array ||
                    innerOutputs.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("LangFlow response missing inner 'outputs' array.");
                }

                // Track already-seen URLs so we return only the first unique one.
                var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var outputItem in innerOutputs.EnumerateArray())
                {
                    if (!outputItem.TryGetProperty("results", out var results)) continue;
                    if (!results.TryGetProperty("message", out var message)) continue;
                    if (!message.TryGetProperty("text", out var textProp)) continue;

                    var text = textProp.GetString();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    // The text may contain a ```json ... ``` code block with a "url" field.
                    var url = TryExtractUrlFromCodeBlock(text);
                    if (url == null) continue;

                    if (seenUrls.Add(url))
                    {
                        // First unique URL found — return immediately.
                        return url;
                    }
                }
            }

            throw new InvalidOperationException(
                "LangFlow response did not contain a valid image URL. " +
                "Ensure the flow is configured correctly and returned a success JSON with a 'url' property.");
        }

        /// <summary>
        /// Strips a ```json ... ``` markdown fence from the text and extracts the 'url' property.
        /// Returns null if the text does not match the expected format.
        /// </summary>
        private string? TryExtractUrlFromCodeBlock(string text)
        {
            // Match content inside ```json ... ``` (case-insensitive, multi-line)
            var match = Regex.Match(text, @"```json\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
            string jsonContent;

            if (match.Success)
            {
                jsonContent = match.Groups[1].Value.Trim();
            }
            else
            {
                // Fallback: attempt to parse the raw text as JSON.
                jsonContent = text.Trim();
            }

            try
            {
                using var innerDoc = JsonDocument.Parse(jsonContent);
                var root = innerDoc.RootElement;

                if (root.TryGetProperty("success", out var successProp) &&
                    successProp.ValueKind == JsonValueKind.True &&
                    root.TryGetProperty("url", out var urlProp))
                {
                    var url = urlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                        return url;
                }
            }
            catch (JsonException)
            {
                // Not a JSON block — not the output we need.
            }

            return null;
        }

        /// <summary>
        /// Maps a Content-Type media type to a file extension.
        /// The Content-Type is the single source of truth — never the URL.
        /// </summary>
        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "image/png"     => "png",
                "image/jpeg"    => "jpg",
                "image/jpg"     => "jpg",
                "image/webp"    => "webp",
                "image/gif"     => "gif",
                "image/bmp"     => "bmp",
                "image/tiff"    => "tiff",
                "image/svg+xml" => "svg",
                "image/avif"    => "avif",
                _ => "bin" // unknown image type — store with generic extension
            };
        }
    }

    // -----------------------------------------------------------------------
    // Small helper to avoid pulling in a full utility package.
    // -----------------------------------------------------------------------
    internal static class StringExtensions
    {
        internal static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
