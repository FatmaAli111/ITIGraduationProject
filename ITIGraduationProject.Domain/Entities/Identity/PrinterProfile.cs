using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.Identity
{
    public class PrinterProfile : BaseAuditableEntity
    {
        public Guid UserId { get; set; }

        public FabricType SupportedFabrics { get; set; }

        public PrintMethodType SupportedPrintMethods { get; set; }

        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();

    }
}
