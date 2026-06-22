using ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization;
using ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts;
using ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Application.Common.Mappings
{
    public class StudioMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            config.NewConfig<Product, StudioProductDetailDto>()
            .Map(dest => dest.AvailableMockups, src => src.ProductImages
                .Where(img => !string.IsNullOrEmpty(img.PrintableZoneJson))
                .OrderByDescending(img => img.IsPrimary)
                .ThenBy(img => img.DisplayOrder));

            config.NewConfig<ProductImage, StudioMockupDetailDto>()
            .Map(dest => dest.Color, src => src.Color)
            .Map(dest => dest.ViewAngle, src => src.ViewAngle != null ? src.ViewAngle.ToString() : string.Empty)
            .Map(dest => dest.ImageUrl, src => src.ImageUrl)
            .Map(dest => dest.PrintableZone, src => !string.IsNullOrEmpty(src.PrintableZoneJson)
                ? JsonSerializer.Deserialize<PrintableZoneDetailDto>(src.PrintableZoneJson, jsonOptions)
                : null);


            config.NewConfig<Product, StudioProductListItemDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.BasePrice, src => src.BasePrice)
            .Map(dest => dest.ThumbnailImageUrl, src => src.ProductImages
                .Where(img => !string.IsNullOrEmpty(img.PrintableZoneJson))
                .OrderByDescending(img => img.IsPrimary)
                .ThenBy(img => img.DisplayOrder)
                .Select(img => img.ImageUrl)
                .FirstOrDefault() ?? string.Empty);

            config.NewConfig<AiChatSession, AiChatSessionDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.CurrentDesignId, src => src.CurrentDesignId)
                .Map(dest => dest.SessionType, src => (int)src.SessionType)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            config.NewConfig<AiChatMessage, AiChatMessageDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.SessionId, src => src.AiChatSessionId)
                .Map(dest => dest.Sender, src => src.Sender)
                .Map(dest => dest.MessageText, src => src.MessageText)
                .Map(dest => dest.SentAt, src => src.SentAt);

        }
    
    }
}
