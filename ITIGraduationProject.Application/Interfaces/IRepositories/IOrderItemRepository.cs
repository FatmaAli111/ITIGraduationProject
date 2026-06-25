using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.Repositories
{
    public interface IOrderItemRepository : IGenericRepo<OrderItem>
    {
    }
}
