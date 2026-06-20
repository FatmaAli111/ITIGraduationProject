using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;

namespace ITIGraduationProject.Application.CQRS.Queries;

public class GetDesignByIdQuery : IRequest<Response<DesignDetailDTO>>
{
    public Guid DesignId { get; set; }
}
