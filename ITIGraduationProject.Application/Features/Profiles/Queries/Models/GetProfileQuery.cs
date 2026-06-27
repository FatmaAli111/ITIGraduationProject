using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Profiles;

namespace ITIGraduationProject.Application.Features.Profiles.Queries.Models
{
    public class GetProfileQuery : IRequest<Response<ProfileDTO>>
    {
        public string Email { get; set; }

        public GetProfileQuery(string email)
        {
            Email = email;
        }
    }
}