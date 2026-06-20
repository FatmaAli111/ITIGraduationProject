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
    public class CreateCategoryCommandHandler
    : ResponseHandler,
      IRequestHandler<CreateCategoryCommand, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _uow;

        public CreateCategoryCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<CategoryDto>> Handle(
            CreateCategoryCommand cmd, CancellationToken ct)
        {
            var IsCategoryExist = _uow.Categories.GetTableNoTracking().Any(c => c.Name == cmd.Name && !c.IsDeleted);
            if (IsCategoryExist)
            {
                return BadRequest<CategoryDto>("Category already exists");
            }
            var category = new Category
            {
                Name = cmd.Name,
                Description = cmd.Description,
                PrintableAreaConfig = cmd.PrintableAreaConfig,
                ImageUrl = cmd.ImageUrl,
            };

            await _uow.Categories.AddAsync(category);
            await _uow.SaveChangesAsync();

            return Created(category.Adapt<CategoryDto>());
        }
    }
}
