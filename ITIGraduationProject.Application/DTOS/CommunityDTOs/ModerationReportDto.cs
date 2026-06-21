using System;

namespace ITIGraduationProject.Application.DTOS.CommunityDTOs
{
    public class ModerationReportDto
    {
        public Guid Id { get; set; }
        public Guid ReporterUserId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public Guid TargetTemplateId { get; set; }
        public string TargetTemplateName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
