using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Commands.Handlers
{
    public class GenerateOnboardingTemplatesCommandHandler
        : ResponseHandler,
          IRequestHandler<GenerateOnboardingTemplatesCommand, Response<List<TemplateDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAILayerClient _ai;
        private readonly ICurrentUserService _currentUser;

        private static readonly string[] VariationHints = new[]
        {
            "clean line-art and a single bold accent color",
            "grungy distressed texture with layered elements",
            "bold typography combined with a graphic motif"
        };

        public GenerateOnboardingTemplatesCommandHandler(
            IUnitOfWork uow, IAILayerClient ai, ICurrentUserService currentUser)
            => (_uow, _ai, _currentUser) = (uow, ai, currentUser);

        public async Task<Response<List<TemplateDto>>> Handle(
            GenerateOnboardingTemplatesCommand cmd, CancellationToken ct)
        {
            // 1. Load user with preferences
            var user = await _uow.Users
                .GetTableNoTracking()
                .Include(u => u.UserPreferences)
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

            if (user?.UserPreferences is null)
                return BadRequest<List<TemplateDto>>(
                    "Complete preference onboarding before generating templates.");

            var prefs = user.UserPreferences;

            // 2. Try to get a product — if none exist, use defaults
            var product = await _uow.Products
                .GetTableNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(ct);

            var productName = product?.Name ?? "Tshirt";
            var categoryName = product?.Category?.Name ?? "Tshirts";
            var categoryId = product?.CategoryId ?? Guid.Empty;

            Console.WriteLine($"[OnboardingTemplates] Starting generation for user {_currentUser.UserId}");
            Console.WriteLine($"[OnboardingTemplates] Prefs: Colors={prefs.FavoriteColors}, Interests={prefs.Interests}, Design={prefs.DesignPreference}");
            Console.WriteLine($"[OnboardingTemplates] Using: {productName} / {categoryName}");

            // 3. Generate 3 templates in parallel
            var tasks = VariationHints.Select((hint, index) =>
                GenerateSingleTemplateAsync(productName, categoryName, categoryId, prefs, hint, index, ct));

            var results = await Task.WhenAll(tasks);

            var templates = results.Where(t => t is not null).ToList();
            Console.WriteLine($"[OnboardingTemplates] {templates.Count} out of 3 generated successfully");

            if (templates.Count == 0)
                return BadRequest<List<TemplateDto>>(
                    "AI service could not generate templates. Please try again later.");

            // 4. Save all templates
            foreach (var template in templates)
            {
                await _uow.Templates.AddAsync(template);
            }
            await _uow.SaveChangesAsync();

            var dtos = templates.Select(t => t.Adapt<TemplateDto>()).ToList();
            return Success(dtos, $"{templates.Count} personalized templates generated successfully.");
        }

        private async Task<Template?> GenerateSingleTemplateAsync(
            string productName,
            string categoryName,
            Guid categoryId,
            Domain.Entities.Identity.UserPreferences prefs,
            string variationHint,
            int index,
            CancellationToken ct)
        {
            try
            {
                var request = new AIGenerateRequest
                {
                    ProductName = productName,
                    CategoryName = categoryName,
                    StyleType = prefs.StyleType ?? "Sporty",
                    FavoriteColors = prefs.FavoriteColors ?? "Blue",
                    Interests = prefs.Interests ?? "General",
                    DesignPreference = $"{prefs.DesignPreference ?? "clean"}, {variationHint}",
                };

                var imageUrl = await _ai.GenerateTemplateImageAsync(request, ct);

                var fileName = $"{Guid.NewGuid()}.png";
                var savePath = Path.Combine("wwwroot", "uploads", "templates", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl, ct);
                await File.WriteAllBytesAsync(savePath, imageBytes, ct);

                var permanentUrl = $"/uploads/templates/{fileName}";

                return new Template
                {
                    CategoryId = categoryId,
                    CreatorUserId = _currentUser.UserId,
                    Name = $"AI Template {index + 1} — {DateTime.UtcNow:MMM dd}",
                    StyleTags = $"{prefs.StyleType ?? "Sporty"}, {variationHint.Split(' ').First()}",
                    PreviewImageURL = permanentUrl,
                    IsPublic = false,
                    LikesCount = 0,
                    RemixesCount = 0,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OnboardingTemplate {index + 1}] Generation FAILED: {ex.Message}");
                Console.WriteLine($"[OnboardingTemplate {index + 1}] Stack: {ex.StackTrace}");
                return null;
            }
        }
    }
}