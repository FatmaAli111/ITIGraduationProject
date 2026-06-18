#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Designs;

namespace ITIGraduationProject.Domain.Entities.Products;

public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PrintableAreaConfig { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageFileName { get; set; }
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    public ICollection<Template> Templates { get; set; } = new HashSet<Template>();
}
