using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Handlers
{
    public class GetPrinterProfilesQueryHandler : IRequestHandler<GetPrinterProfilesQuery, PaginatedResult<PrinterProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPrinterProfilesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PrinterProfileDto>> Handle(GetPrinterProfilesQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.PrinterProfiles
                .GetTableNoTracking()
                .Include(p => p.User)
                .Where(p => !p.IsDeleted);

            var paginatedProfiles = await query.ToPaginatedListAsync(request.PageNumber, request.PageSize);

            var dtos = _mapper.Map<List<PrinterProfileDto>>(paginatedProfiles.Data);

            return PaginatedResult<PrinterProfileDto>.Success(
                dtos,
                paginatedProfiles.TotalCount,
                paginatedProfiles.CurrentPage,
                paginatedProfiles.PageSize);
        }
    }
}
