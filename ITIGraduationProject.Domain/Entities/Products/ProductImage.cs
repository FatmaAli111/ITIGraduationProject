using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.Products
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public ViewAngle? ViewAngle { get; set; }
        public Product Product { get; set; } = null!;
    }
}
