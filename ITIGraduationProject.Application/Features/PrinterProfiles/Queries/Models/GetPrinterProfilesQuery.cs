using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Models
{
    public class GetPrinterProfilesQuery : IRequest<PaginatedResult<PrinterProfileDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
