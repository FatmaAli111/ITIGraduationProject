using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Mapster;

namespace ITIGraduationProject.Application.Features.Community.Mapping
{
    public class CommunityMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Template, FeedItemDto>()
                .Map(dest => dest.CreatorName, src => src.CreatorUser.Name)
                .Map(dest => dest.CreatorProfileImageUrl, src => src.CreatorUser.ProfileImageUrl)
                .Map(dest => dest.CommentCount,
                    src => src.CommunityInteractions.Count(
                        ci => ci.InteractionType == InteractionType.Comment));

            config.NewConfig<CommunityInteraction, CommentDto>()
                .Map(dest => dest.Content, src => src.Content ?? string.Empty)
                .Map(dest => dest.UserName, src => src.User.Name)
                .Map(dest => dest.UserProfileImageUrl, src => src.User.ProfileImageUrl);

            config.NewConfig<ModerationReport, ModerationReportDto>()
                .Map(dest => dest.ReporterName, src => src.ReporterUser.Name)
                .Map(dest => dest.TargetTemplateName, src => src.TargetTemplate.Name)
                .Map(dest => dest.Status, src => src.Status.ToString());
        }
    }
}
