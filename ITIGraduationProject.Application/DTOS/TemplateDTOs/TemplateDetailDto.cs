using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.TemplateDTOs
{
    public class TemplateDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PreviewImageURL { get; set; } = string.Empty;
        public string? StyleTags { get; set; }
        public bool IsPublic { get; set; }
        public int LikesCount { get; set; }
        public int RemixesCount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid CreatorUserId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string CanvasStateJSON { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
