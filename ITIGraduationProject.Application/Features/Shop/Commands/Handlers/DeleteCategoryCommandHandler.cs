using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class DeleteCategoryCommandHandler
    : ResponseHandler,
      IRequestHandler<DeleteCategoryCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;

        public DeleteCategoryCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<string>> Handle(
            DeleteCategoryCommand cmd, CancellationToken ct)
        {
            var category = await _uow.Categories.GetByIdAsync(cmd.Id);

            if (category is null || category.IsDeleted)
                return NotFound<string>("Category not found");

            // Guard: don't delete a category that still has active products
            var hasProducts = await _uow.Products
                .GetTableNoTracking()
                .AnyAsync(p => p.CategoryId == cmd.Id && !p.IsDeleted, ct);

            if (hasProducts)
                return BadRequest<string>("Cannot delete a category that still has products");

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;

            _uow.Categories.Update(category);
            await _uow.SaveChangesAsync();

            return Deleted<string>();
        }
    }
}
