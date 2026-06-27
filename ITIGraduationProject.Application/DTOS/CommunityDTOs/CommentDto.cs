using System;

namespace ITIGraduationProject.Application.DTOS.CommunityDTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; }
    }
}
