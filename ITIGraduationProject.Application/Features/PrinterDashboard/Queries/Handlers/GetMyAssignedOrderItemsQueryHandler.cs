using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
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
    public class GetMyAssignedOrderItemsQueryHandler : IRequestHandler<GetMyAssignedOrderItemsQuery, PaginatedResult<PrinterOrderItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetMyAssignedOrderItemsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<PaginatedResult<PrinterOrderItemDto>> Handle(GetMyAssignedOrderItemsQuery request, CancellationToken cancellationToken)
        {
            var printerProfile = await _unitOfWork.PrinterProfiles
                .GetTableNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId);

            if (printerProfile == null)
                return new PaginatedResult<PrinterOrderItemDto>(new List<PrinterOrderItemDto>())
                {
                    Succeeded = false,
                    Messages = new List<string> { "No printer profile found for this user. Create one first." }
                };

            var query = _unitOfWork.OrderItems
                .GetTableNoTracking()
                .Include(oi => oi.Order)
                .Where(oi => oi.PrinterProfileId == printerProfile.Id);

            if (request.Status.HasValue)
            {
                query = query.Where(oi => oi.Status == request.Status.Value);
            }

            var paginatedItems = await query
                .OrderByDescending(oi => oi.Order.CreatedAt)
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            var dtos = paginatedItems.Data.Select(oi => new PrinterOrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                OrderNumber = oi.Order.OrderNumber,
                DesignSnapshotUrl = oi.SnapshotImageURL,
                Quantity = oi.Quantity,
                Status = oi.Status.ToString()
            }).ToList();

            return PaginatedResult<PrinterOrderItemDto>.Success(
                dtos,
                paginatedItems.TotalCount,
                paginatedItems.CurrentPage,
                paginatedItems.PageSize);
        }
    }
}
