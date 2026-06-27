using System;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class CartItemRepository : GenericRepo<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
