using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;

namespace ITIGraduationProject.Application.CQRS.Commands;

public class UpdateDesignCommand : IRequest<Response<DesignDetailDTO>>
{
    public Guid DesignId { get; set; }
    public Guid UserId { get; set; }
    public string? DesignName { get; set; }
    public string? Colors { get; set; }
    public string? PrintLocation { get; set; }
}
