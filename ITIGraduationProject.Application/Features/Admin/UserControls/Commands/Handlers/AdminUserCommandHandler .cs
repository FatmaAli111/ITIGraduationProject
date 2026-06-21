using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Handlers
{
    public class AdminUserCommandHandler :
         IRequestHandler<InviteUserCommand, Response<string>>,
         IRequestHandler<UpdateUserCommand, Response<string>>,
         IRequestHandler<ChangeUserStatusCommand, Response<string>>
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IMapper _mapper;

        public AdminUserCommandHandler(IAdminUserService adminUserService, IMapper mapper)
        {
            _adminUserService = adminUserService;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<InviteUserRequestDTO>(request);
            return await _adminUserService.InviteUserAsync(dto);
        }

        public async Task<Response<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<UpdateUserRequestDTO>(request);
            return await _adminUserService.UpdateUserAsync(request.Id, dto);
        }

        public async Task<Response<string>> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            return await _adminUserService.ChangeUserStatusAsync(request.Id, request.IsActive);
        }
    }
}
