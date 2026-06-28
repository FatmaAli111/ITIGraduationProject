namespace ITIGraduationProject.Service.ReportGenerator;

public sealed class ReportGeneratorSettings
{
    public const string SectionName = "ReportGenerator";

    public string BaseUrl { get; set; } = string.Empty;
    public string FlowId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string AnalyticsToolComponentId { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 180;
}
