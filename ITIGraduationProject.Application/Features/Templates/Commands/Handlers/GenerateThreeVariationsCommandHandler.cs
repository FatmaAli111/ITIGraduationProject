using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IRepositories;
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
    public class GenerateThreeVariationsCommandHandler
    : ResponseHandler,
      IRequestHandler<GenerateThreeVariationsCommand, Response<GenerateVariationsResponseDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAILayerClient _ai;
        private readonly ICurrentUserService _currentUser;

        public GenerateThreeVariationsCommandHandler(
            IUnitOfWork uow, IAILayerClient ai, ICurrentUserService currentUser)
            => (_uow, _ai, _currentUser) = (uow, ai, currentUser);

        public async Task<Response<GenerateVariationsResponseDto>> Handle(
    GenerateThreeVariationsCommand cmd, CancellationToken ct)
        {
            // 1. Load user preferences
            var user = await _uow.Users
                .GetTableNoTracking()
                .Include(u => u.UserPreferences)
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

            if (user?.UserPreferences is null)
                return BadRequest<GenerateVariationsResponseDto>(
                    "Complete preference onboarding before generating templates");

            var prefs = user.UserPreferences;

            // 2. Get category from product if provided
            Guid? categoryId = null;
            if (cmd.ProductId.HasValue)
            {
                var product = await _uow.Products.GetByIdAsync(cmd.ProductId.Value);
                if (product is null)
                    return NotFound<GenerateVariationsResponseDto>("Product not found");
                categoryId = product.CategoryId;
            }

            // 3. Call AI layer — returns 3 variations
            List<GeneratedVariationResult> variations;
            try
            {
                var aiRequest = new AIGenerateRequest
                {
                    ProductName = "T-Shirt",
                    CategoryName = "Streetwear",
                    StyleType = prefs.StyleType ?? "Sporty",
                    FavoriteColors = prefs.FavoriteColors ?? "Blue",
                    Interests = prefs.Interests ?? "Gym",
                    DesignPreference = prefs.DesignPreference ?? "light",
                };

                variations = await _ai.GenerateThreeVariationsAsync(
                    aiRequest, cmd.UserMessage, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Layer Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return BadRequest<GenerateVariationsResponseDto>(
                    "AI service unavailable. Try again later.");
            }

            // 4. Download images, save to wwwroot, and create Template entities
            var variationLabels = new[] { "Line-Art Edition", "Distressed Edition", "Type & Motif" };
            var responseDto = new GenerateVariationsResponseDto();
            var downloadClient = new HttpClient();

            for (int i = 0; i < variations.Count; i++)
            {
                var variation = variations[i];
                string permanentUrl;

                try
                {
                    var fileName = $"{Guid.NewGuid()}.png";
                    var savePath = Path.Combine("wwwroot", "uploads", "templates", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                    var imageBytes = await downloadClient.GetByteArrayAsync(variation.ImageUrl, ct);
                    await File.WriteAllBytesAsync(savePath, imageBytes, ct);

                    permanentUrl = $"/uploads/templates/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image download error for variation {i + 1}: {ex.Message}");
                    permanentUrl = variation.ImageUrl;
                }

                var template = new Template
                {
                    CategoryId = categoryId ?? Guid.Empty,
                    CreatorUserId = _currentUser.UserId,
                    Name = string.IsNullOrWhiteSpace(variation.ConceptName)
                        ? $"AI Design — {variationLabels[i]} — {DateTime.UtcNow:MMM dd HH:mm}"
                        : $"{variation.ConceptName} — {DateTime.UtcNow:MMM dd HH:mm}",
                    StyleTags = prefs.StyleType ?? "Sporty",
                    PreviewImageURL = permanentUrl,
                    IsPublic = false,
                    LikesCount = 0,
                    RemixesCount = 0,
                };

                await _uow.Templates.AddAsync(template);

                responseDto.Variations.Add(new GeneratedVariationDto
                {
                    ConceptName = variation.ConceptName,
                    ImagePrompt = variation.ImagePrompt,
                    ImageUrl = permanentUrl,
                    VariationLabel = variationLabels[i],
                });
            }

            await _uow.SaveChangesAsync();

            return Success(responseDto, "3 templates generated successfully");
        }
    }
}