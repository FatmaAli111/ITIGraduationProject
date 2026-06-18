public interface IAILayerClient
{
    Task<string> GenerateTemplateImageAsync(
        AIGenerateRequest request, CancellationToken ct = default);
}

public class AIGenerateRequest
{
    public string ProductName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string StyleType { get; set; } = "";
    public string FavoriteColors { get; set; } = "";
    public string Interests { get; set; } = "";
    public string DesignPreference { get; set; } = "";
}