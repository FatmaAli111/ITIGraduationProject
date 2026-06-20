using ITIGraduationProject.Application.DTOS.Profiles;
using ITIGraduationProject.Domain.Entities.Identity;
using Mapster;


namespace ITIGraduationProject.Application.Features.Profiles.Commands.Mapping
{
    public class ProfileMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, ProfileDTO>()
                .Map(dest => dest.ProfilePictureUrl, src => src.ProfileImageUrl);
            // dest => Destination
        }
    }
}
