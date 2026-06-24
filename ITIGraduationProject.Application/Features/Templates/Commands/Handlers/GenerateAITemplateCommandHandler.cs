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
    public class GenerateAITemplateCommandHandler
    : ResponseHandler,
      IRequestHandler<GenerateAITemplateCommand, Response<TemplateDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAILayerClient _ai;
        private readonly ICurrentUserService _currentUser;

        public GenerateAITemplateCommandHandler(
            IUnitOfWork uow, IAILayerClient ai, ICurrentUserService currentUser)
            => (_uow, _ai, _currentUser) = (uow, ai, currentUser);

        public async Task<Response<TemplateDto>> Handle(
            GenerateAITemplateCommand cmd, CancellationToken ct)
        {
            // 1. Load product
            var product = await _uow.Products.GetByIdAsync(cmd.ProductId);
            if (product is null)
                return NotFound<TemplateDto>("Product not found");

            // 2. Load user preferences
            var user = await _uow.Users
                .GetTableNoTracking()
                .Include(u => u.UserPreferences)
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

            if (user?.UserPreferences is null)
                return BadRequest<TemplateDto>(
                    "Complete preference onboarding before generating templates");

            var prefs = user.UserPreferences;

            // 3. Call AI layer — returns PNG image URL
            string imageUrl;
            try
            {
                imageUrl = await _ai.GenerateTemplateImageAsync(new AIGenerateRequest
                {
                    ProductName = product.Name,
                    CategoryName = product.Category?.Name ?? "Template1",
                    StyleType = prefs.StyleType ?? "Sporty",
                    FavoriteColors = prefs.FavoriteColors ?? "Blue",
                    Interests = prefs.Interests ?? "Gym",
                    DesignPreference = prefs.DesignPreference ?? "light",
                }, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Layer Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return BadRequest<TemplateDto>("AI service unavailable. Try again later.");
            }

            // 4. Download the image and save it to wwwroot
            var fileName = $"{Guid.NewGuid()}.png";
            var savePath = Path.Combine("wwwroot", "uploads", "templates", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl, ct);
            await File.WriteAllBytesAsync(savePath, imageBytes, ct);

            var permanentUrl = $"/uploads/templates/{fileName}"; // relative path, served by static files middleware

            var template = new Template
            {
                CategoryId = product.CategoryId,
                CreatorUserId = _currentUser.UserId,
                Name = $"AI Design — {DateTime.UtcNow:MMM dd HH:mm}",
                StyleTags = prefs.StyleType ?? "Sporty",
                PreviewImageURL = permanentUrl,   // ← the PNG path saved in wwwroot
                IsPublic = false,
                LikesCount = 0,
                RemixesCount = 0,
            };

            await _uow.Templates.AddAsync(template);
            await _uow.SaveChangesAsync();

            return Created(template.Adapt<TemplateDto>());
        }
    }
}
