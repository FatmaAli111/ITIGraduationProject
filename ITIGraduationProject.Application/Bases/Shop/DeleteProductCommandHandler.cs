using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Shop.CQRS;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Shop
{
    public class DeleteProductCommandHandler
    : ResponseHandler,
      IRequestHandler<DeleteProductCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;

        public DeleteProductCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<string>> Handle(
            DeleteProductCommand cmd, CancellationToken ct)
        {
            var product = await _uow.Products.GetByIdAsync(cmd.Id);

            if (product is null || product.IsDeleted)
                return NotFound<string>("Product not found");

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            _uow.Products.Update(product);
            await _uow.SaveChangesAsync();

            return Deleted<string>();
        }
    }
}
