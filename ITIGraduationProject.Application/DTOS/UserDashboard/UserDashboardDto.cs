using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.DTOS.UserDashboard
{
    public class UserDashboardDto
    {
        public string GreetingName { get; set; } = string.Empty;
        public UserDashboardStatsDto Stats { get; set; } = new();
        public UserDashboardFeaturedDraftDto? FeaturedDraft { get; set; }
        public UserDashboardActiveOrderDto? ActiveOrder { get; set; }
        public List<UserDashboardRecommendationDto> Recommendations { get; set; } = new();
        public List<UserDashboardActivityItemDto> RecentActivity { get; set; } = new();
        public List<UserDashboardChecklistItemDto> DesignChecklist { get; set; } = new();
    }

    public class UserDashboardStatsDto
    {
        public int SavedDesigns { get; set; }
        public string? SavedDesignsDelta { get; set; }
        public int ActiveOrders { get; set; }
        public string? ActiveOrdersDelta { get; set; }
        public int CommunityLikes { get; set; }
        public string? CommunityLikesDelta { get; set; }
    }

    public class UserDashboardFeaturedDraftDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string PreviewImageUrl { get; set; } = string.Empty;
        public int Progress { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class UserDashboardActiveOrderDto
    {
        public Guid Id { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Eta { get; set; }
    }

    public class UserDashboardRecommendationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int Reviews { get; set; }
        public List<string> Colors { get; set; } = new();
        public string? Badge { get; set; }
        public string Reason { get; set; } = "Recommended for your saved styles";
    }

    public class UserDashboardActivityItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserDashboardChecklistItemDto
    {
        public string Label { get; set; } = string.Empty;
        public bool Complete { get; set; }
    }
}
