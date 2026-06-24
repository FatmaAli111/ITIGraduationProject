using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Models
{
    public record CreateProductImageCommand(
     Guid ProductId,
     IFormFile ImageFile,
     int? Color,
     int? ViewAngle,
     string? PrintableZoneJson,
     bool IsPrimary,
     int DisplayOrder
 ) : IRequest<Guid>;
}
