using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers.Shop.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public string CategoryName { get; set; }
        public string PreviewImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public decimal AverageRating { get; set; }
    }
}
