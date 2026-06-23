using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{//the error in line 11 is be
    public class ProductImageRepository : GenericRepo<ProductImage> , IProductImageRepository
    {
        public ProductImageRepository(AppDbContext context) : base(context)
        {
        }
        
    }
}
