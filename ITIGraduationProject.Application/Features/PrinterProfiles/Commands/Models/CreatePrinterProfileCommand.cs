using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Models
{
    public class CreatePrinterProfileCommand : IRequest<Response<PrinterProfileDto>>
    {
        public FabricType SupportedFabrics { get; set; }
        public PrintMethodType SupportedPrintMethods { get; set; }
    }
}
