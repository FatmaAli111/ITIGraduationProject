using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IRepositories
{
    public interface IAILayerClient
    {
        Task<string> GenerateTemplateImageAsync(
            AIGenerateRequest request, CancellationToken ct = default);

        Task<List<GeneratedVariationResult>> GenerateThreeVariationsAsync(
            AIGenerateRequest request, string userMessage, CancellationToken ct = default);
    }

    public class AIGenerateRequest
    {
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string StyleType { get; set; } = "";
        public string FavoriteColors { get; set; } = "";
        public string Interests { get; set; } = "";
        public string DesignPreference { get; set; } = "";
    }

    public class GeneratedVariationResult
    {
        public string ConceptName { get; set; } = "";
        public string ImagePrompt { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}