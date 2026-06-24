using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.TemplateDTOs
{
    public class GeneratedVariationDto
    {
        public string ConceptName { get; set; } = "";
        public string ImagePrompt { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string VariationLabel { get; set; } = "";
    }

    public class GenerateVariationsResponseDto
    {
        public List<GeneratedVariationDto> Variations { get; set; } = new();
    }
}