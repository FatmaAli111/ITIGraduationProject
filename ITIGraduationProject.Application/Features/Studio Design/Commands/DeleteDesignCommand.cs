using MediatR;
using ITIGraduationProject.Application.Bases;

namespace ITIGraduationProject.Application.CQRS.Commands;

public class DeleteDesignCommand : IRequest<Response<bool>>
{
    public Guid DesignId { get; set; }
    public Guid UserId { get; set; }
}
