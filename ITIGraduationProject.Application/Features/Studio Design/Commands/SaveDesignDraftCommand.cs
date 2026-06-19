using MediatR;
using ITIGraduationProject.Application.Bases;

namespace ITIGraduationProject.Application.CQRS.Commands;

public class SaveDesignDraftCommand : IRequest<Response<bool>>
{
    public Guid UserId { get; set; }
    public Guid DesignId { get; set; }
}
