using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.FilesServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class CreateProductImageCommandHandler : IRequestHandler<CreateProductImageCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public CreateProductImageCommandHandler(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<Guid> Handle(CreateProductImageCommand request, CancellationToken cancellationToken)
        {
            var productExists = await _unitOfWork.Products.GetTableNoTracking()
                .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

            if (!productExists)
                throw new KeyNotFoundException($"Product with ID {request.ProductId} identity not found.");

            string imageUrl = await _fileService.UploadFileAsync(request.ImageFile, "products");

            if (request.IsPrimary)
                await ResetPrimaryImagesAsync(request.ProductId, cancellationToken);

            var productImage = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                ImageUrl = imageUrl,
                Color = (ProductAvailableColors?)request.Color,
                ViewAngle = (ViewAngle?)request.ViewAngle,
                PrintableZoneJson = request.PrintableZoneJson,
                IsPrimary = request.IsPrimary,
                DisplayOrder = request.DisplayOrder
            };
            await _unitOfWork.ProductImages.AddAsync(productImage);
            await _unitOfWork.SaveChangesAsync();

            return productImage.Id;
        }

        private async Task ResetPrimaryImagesAsync(Guid productId, CancellationToken cancellationToken)
        {
            var primaryImages = await _unitOfWork.ProductImages.GetTableAsTracking()
                .Where(img => img.ProductId == productId && img.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var img in primaryImages)
            {
                img.IsPrimary = false;
            }
        }
    }
}
