using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;

public class UpdateDesignCommandHandler : IRequestHandler<UpdateDesignCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _mediator;

    public UpdateDesignCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ISender mediator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task Handle(UpdateDesignCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var design = await _unitOfWork.Designs.GetByIdAsync(request.Id);

        if (design == null || design.UserId != userId)
        {
            throw new KeyNotFoundException($"Entity \"{nameof(Design)}\" ({request.Id}) was not found or you do not have permission to modify it.");
        }

        var createCommand = new CreateDesignCommand(
            Id: request.Id,
            ProductId: design.ProductId,
            TemplateId: design.TemplateId,
            CanvasStateJSON: request.CanvasStateJSON,
            Base64Snapshot: request.Base64Snapshot,
            Base64Front: request.Base64Front,
            Base64Back: request.Base64Back,
            SelectedSize: request.SelectedSize,
            SelectedFabric: request.SelectedFabric,
            SelectedPrintMethod: request.SelectedPrintMethod,
            SelectedColor: request.SelectedColor,
            Assets: request.Assets
        );

        await _mediator.Send(createCommand, cancellationToken);
    }
}
