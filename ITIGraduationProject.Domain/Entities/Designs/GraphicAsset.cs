#nullable enable
using System;
using System.Collections.Generic;
using ITIGraduationProject.Domain.Common;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
namespace ITIGraduationProject.Domain.Entities.Designs;

public class GraphicAsset : BaseAuditableEntity
{
    public Guid UploadedByAdminId { get; set; }
    public string Name { get; set; } = string.Empty;
    public GraphicAssetType Type { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Tags { get; set; }

    public ICollection<Design> Designs { get; set; } = new HashSet<Design>();
}
