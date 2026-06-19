using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepo<Category>
    {
    }
}
