using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class CreateProductCommandHandler
    : ResponseHandler,
      IRequestHandler<CreateProductCommand, Response<ProductDto>>
    {
        private readonly IUnitOfWork _uow;

        public CreateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<ProductDto>> Handle(
            CreateProductCommand cmd, CancellationToken ct)
        {
            var product = new Product
            {
                CategoryId = cmd.CategoryId,
                Name = cmd.Name,
                BasePrice = cmd.BasePrice,
                AvailableColors = cmd.AvailableColors,
                PreviewImageURL = cmd.PreviewImageURL,
                IsAvailable = cmd.IsAvailable,
                StockStatus = cmd.StockStatus,
                AverageRating = 0,
                ReviewCount = 0,
            };

            await _uow.Products.AddAsync(product);
            await _uow.SaveChangesAsync();

            return Created(product.Adapt<ProductDto>());
        }
    }
}
