using ITIGraduationProject.Application.Bases;
using MediatR;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels
{
    public class ResendInvitationCommand : IRequest<Response<string>>
    {
        public Guid Id { get; set; }
    }
}
