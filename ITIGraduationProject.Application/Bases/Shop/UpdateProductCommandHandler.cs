using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Shop.CQRS;
using ITIGraduationProject.Application.Wrapers.Shop.DTOs;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Shop
{
    public class UpdateProductCommandHandler
    : ResponseHandler,
      IRequestHandler<UpdateProductCommand, Response<ProductDto>>
    {
        private readonly IUnitOfWork _uow;

        public UpdateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<ProductDto>> Handle(
            UpdateProductCommand cmd, CancellationToken ct)
        {
            var product = await _uow.Products.GetByIdAsync(cmd.Id);

            if (product is null || product.IsDeleted)
                return NotFound<ProductDto>("Product not found");

            if (cmd.Name is not null) product.Name = cmd.Name;
            if (cmd.BasePrice.HasValue) product.BasePrice = cmd.BasePrice.Value;
            if (cmd.AvailableColors.HasValue) product.AvailableColors = cmd.AvailableColors.Value;
            if (cmd.PreviewImageURL is not null) product.PreviewImageURL = cmd.PreviewImageURL;
            if (cmd.IsAvailable.HasValue) product.IsAvailable = cmd.IsAvailable.Value;
            if (cmd.StockStatus is not null) product.StockStatus = cmd.StockStatus;
            product.UpdatedAt = DateTime.UtcNow;

            _uow.Products.Update(product);
            await _uow.SaveChangesAsync();

            return Success(product.Adapt<ProductDto>());
        }
    }
}
