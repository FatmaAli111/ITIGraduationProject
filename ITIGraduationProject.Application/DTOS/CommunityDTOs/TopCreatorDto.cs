using System;

namespace ITIGraduationProject.Application.DTOS.CommunityDTOs
{
    public class TopCreatorDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public int TotalLikes { get; set; }
        public int TotalRemixes { get; set; }
        public int TemplateCount { get; set; }
    }
}
