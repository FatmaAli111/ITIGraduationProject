using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Domain.Entities.Identity;
using Mapster;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Mapping
{
    public class PrinterProfileMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PrinterProfile, PrinterProfileDto>()
                .Map(dest => dest.PrinterName, src => src.User.Name);
        }
    }
}
