using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;

namespace ITIGraduationProject.Application.Features.Templates.Mapping
{
    public class TemplateMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Template, TemplateDto>()
                .Map(dest => dest.CreatorName, src => src.CreatorUser.Name)
                .Map(dest => dest.CategoryName, src => src.Category.Name);

            config.NewConfig<Template, TemplateDetailDto>()
                .Map(dest => dest.CreatorName, src => src.CreatorUser.Name)
                .Map(dest => dest.CategoryName, src => src.Category.Name);
        }
    }
}
