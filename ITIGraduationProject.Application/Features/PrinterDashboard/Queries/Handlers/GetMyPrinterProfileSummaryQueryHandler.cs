using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Handlers
{
    public class GetMyPrinterProfileSummaryQueryHandler : ResponseHandler, IRequestHandler<GetMyPrinterProfileSummaryQuery, Response<PrinterProfileSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetMyPrinterProfileSummaryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Response<PrinterProfileSummaryDto>> Handle(GetMyPrinterProfileSummaryQuery request, CancellationToken cancellationToken)
        {
            var printerProfile = await _unitOfWork.PrinterProfiles
                .GetTableNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId);

            if (printerProfile == null)
                return NotFound<PrinterProfileSummaryDto>("No printer profile found for this user. Create one first.");

            var assignedItems = await _unitOfWork.OrderItems
                .GetTableNoTracking()
                .Where(oi => oi.PrinterProfileId == printerProfile.Id)
                .ToListAsync();

            var pendingCount = assignedItems.Count(oi => oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.AssignedToPrinter || oi.Status == OrderItemStatus.InProduction);
            var completedCount = assignedItems.Count(oi => oi.Status == OrderItemStatus.Shipped);

            var dto = new PrinterProfileSummaryDto
            {
                ProfileId = printerProfile.Id,
                SupportedFabrics = printerProfile.SupportedFabrics,
                SupportedPrintMethods = printerProfile.SupportedPrintMethods,
                IsActive = printerProfile.IsActive,
                TotalAssignedItems = assignedItems.Count,
                PendingItems = pendingCount,
                CompletedItems = completedCount
            };

            return Success(dto);
        }
    }
}
