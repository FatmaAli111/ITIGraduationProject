using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Common;
namespace ITIGraduationProject.Domain.Entities.Designs
{
    public class DesignImage : BaseEntity
    {
        public Guid DesignId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }

        public virtual Design Design { get; set; } = null!;
    }
}
