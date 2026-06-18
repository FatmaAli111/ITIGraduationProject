#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.Products;

public class Product : BaseAuditableEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public ProductAvailableColors AvailableColors { get; set; }
    public string PreviewImageURL { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Design> Designs { get; set; } = new HashSet<Design>();
    public ICollection<ProductImage> ProductImages { get; set; } = new HashSet<ProductImage>();
}
