using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin.AiReports;
using MediatR;

namespace ITIGraduationProject.Application.Features.Admin.AiReports.Commands.Models;

public sealed class GenerateAiReportCommand : IRequest<Response<AiReportDto>>
{
    public string ReportType { get; set; } = string.Empty;
    public AiReportFiltersDto? Filters { get; set; }
}
