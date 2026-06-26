using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.TemplateDTOs
{
    public class TemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PreviewImageURL { get; set; } = string.Empty;
        public string StyleTags { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int LikesCount { get; set; }
        public int RemixesCount { get; set; }
        public Guid CreatorUserId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
