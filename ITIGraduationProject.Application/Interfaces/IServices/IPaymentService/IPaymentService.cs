using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.ECommerce;

public interface IPaymentService
{
    Task<string> CreatePaymentSessionAsync(Order order);
}
