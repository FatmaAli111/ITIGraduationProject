using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;

public class CreateDesignCommandHandler : IRequestHandler<CreateDesignCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDesignCommandHandler(IUnitOfWork _unitOfWork)
    {
        this._unitOfWork = _unitOfWork;
    }

    public async Task<Guid> Handle(CreateDesignCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products
            .GetTableNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Entity \"{nameof(Product)}\" ({request.ProductId}) was not found.");
        }

        decimal calculatedPrice = product.BasePrice;

        var design = request.Adapt<Design>();

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        return design.Id;
    }
}
