using System;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.ECommerce;

namespace ITIGraduationProject.Application.Repositories
{
    public interface ICouponRepository : IGenericRepo<Coupon>
    {
        Task<Coupon?> GetByCodeAsync(string code);
    }
}
