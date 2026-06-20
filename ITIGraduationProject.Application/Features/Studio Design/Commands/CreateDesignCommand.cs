using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;

namespace ITIGraduationProject.Application.CQRS.Commands;

public class CreateDesignCommand : IRequest<Response<DesignDetailDTO>>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? DesignName { get; set; }
    public string? Colors { get; set; }
    public string? PrintLocation { get; set; }
}
