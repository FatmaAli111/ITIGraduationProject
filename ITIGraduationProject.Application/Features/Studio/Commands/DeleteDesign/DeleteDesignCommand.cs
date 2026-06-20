using MediatR;
using ITIGraduationProject.Application.Bases;

namespace ITIGraduationProject.Application.Features.Studio.Commands.DeleteDesign;

public record DeleteDesignCommand(Guid Id) : IRequest;
