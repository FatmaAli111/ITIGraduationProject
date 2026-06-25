using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models
{
    public class GetMyPrinterProfileSummaryQuery : IRequest<Response<PrinterProfileSummaryDto>>
    {
    }
}
