using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;

namespace ITIGraduationProject.Application.CQRS.Queries;

public class GetUserDesignsQuery : IRequest<Response<List<DesignListDTO>>>
{
    public Guid UserId { get; set; }
}
