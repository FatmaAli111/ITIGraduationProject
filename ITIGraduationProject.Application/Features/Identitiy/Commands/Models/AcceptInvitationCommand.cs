using ITIGraduationProject.Application.Bases;
using MediatR;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class AcceptInvitationCommand : IRequest<Response<string>>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
