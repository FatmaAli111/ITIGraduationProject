using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization
{
    public class StudioProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public List<StudioMockupDetailDto> AvailableMockups { get; set; } = new();
    }
}
