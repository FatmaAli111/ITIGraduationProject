using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : GenericRepo<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
