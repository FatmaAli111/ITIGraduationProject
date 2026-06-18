using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts
{
    public class StudioProductListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string ThumbnailImageUrl { get; set; } = string.Empty;
    }
}
