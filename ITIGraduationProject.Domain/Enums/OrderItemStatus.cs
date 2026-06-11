using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Domain.Enums
{
    public enum OrderItemStatus
    {
        Pending = 1,
        AssignedToPrinter = 2,
        InProduction = 3,
        Ready = 4,
        Shipped = 5
    }
}
