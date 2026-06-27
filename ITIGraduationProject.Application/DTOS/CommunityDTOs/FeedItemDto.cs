using System;

namespace ITIGraduationProject.Application.DTOS.CommunityDTOs
{
    public class FeedItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PreviewImageURL { get; set; } = string.Empty;
        public string? StyleTags { get; set; }
        public int LikesCount { get; set; }
        public int RemixesCount { get; set; }
        public int CommentCount { get; set; }
        public decimal AverageRating { get; set; }
        public Guid CreatorUserId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string? CreatorProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool LikedByCurrentUser { get; set; }
        public bool SavedByCurrentUser { get; set; }
    }
}
