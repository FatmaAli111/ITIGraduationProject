using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class UpdateCategoryCommandHandler
    : ResponseHandler,
      IRequestHandler<UpdateCategoryCommand, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _uow;

        public UpdateCategoryCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<CategoryDto>> Handle(
            UpdateCategoryCommand cmd, CancellationToken ct)
        {
            var category = await _uow.Categories.GetByIdAsync(cmd.Id);

            if (category is null || category.IsDeleted)
                return NotFound<CategoryDto>("Category not found");

            if (cmd.Name is not null) category.Name = cmd.Name;
            if (cmd.Description is not null) category.Description = cmd.Description;
            if (cmd.PrintableAreaConfig is not null) category.PrintableAreaConfig = cmd.PrintableAreaConfig;
            if (cmd.ImageUrl is not null) category.ImageUrl = cmd.ImageUrl;
            category.UpdatedAt = DateTime.UtcNow;

            _uow.Categories.Update(category);
            await _uow.SaveChangesAsync();

            return Success(category.Adapt<CategoryDto>());
        }
    }
}
