using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Identity;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Handlers
{
    public class CreatePrinterProfileCommandHandler : ResponseHandler, IRequestHandler<CreatePrinterProfileCommand, Response<PrinterProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CreatePrinterProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Response<PrinterProfileDto>> Handle(CreatePrinterProfileCommand request, CancellationToken cancellationToken)
        {
            var existingProfile = await _unitOfWork.PrinterProfiles
                .GetTableNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId && !p.IsDeleted);

            if (existingProfile != null)
                return BadRequest<PrinterProfileDto>("Printer profile already exists for this user");

            var printerProfile = new PrinterProfile
            {
                UserId = _currentUser.UserId,
                SupportedFabrics = request.SupportedFabrics,
                SupportedPrintMethods = request.SupportedPrintMethods,
                IsActive = true
            };

            await _unitOfWork.PrinterProfiles.AddAsync(printerProfile);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<PrinterProfileDto>(printerProfile);
            return Created(dto);
        }
    }
}
