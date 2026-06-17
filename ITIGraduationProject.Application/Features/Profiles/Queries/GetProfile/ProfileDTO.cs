using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Profiles.Queries.GetProfile
{
    public class ProfileDTO
    {
    #region StaticData
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } 
        public string Bio { get; set; } 
        public string ProfilePictureUrl { get; set; }
        public string Location { get; set; }
        public DateTime DateJoined { get; set; }
        public bool IsTopProfile { get; set; }
    #endregion
    #region CalculatedData
        public int ItemsPurchasedCount { get; set; }
        public int TotalOrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public int TemplatesCreatedCount { get; set; }
        public double AvgTemplateRating { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    #endregion
    }
}
