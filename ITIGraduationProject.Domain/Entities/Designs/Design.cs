#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.Designs;

public class Design : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? TemplateId { get; set; }
    public string CanvasStateJSON { get; set; } = string.Empty;
    public string SnapshotImageURL { get; set; } = string.Empty;
    public DesignStatus Status { get; set; }
    public ProductSize? SelectedSize { get; set; }
    public FabricType? SelectedFabric { get; set; }
    public PrintMethodType? SelectedPrintMethod { get; set; }
    public ProductAvailableColors SelectedColor { get; set; }
    public decimal CalculatedPrice { get; set; }

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Template? Template { get; set; }

    public ICollection<GraphicAsset> GraphicAssets { get; set; } = new HashSet<GraphicAsset>();
    public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    public ICollection<AiChatSession> AiChatSessions { get; set; } = new HashSet<AiChatSession>();
    public ICollection<DesignImage> DesignImages { get; set; } = new HashSet<DesignImage>();
}
