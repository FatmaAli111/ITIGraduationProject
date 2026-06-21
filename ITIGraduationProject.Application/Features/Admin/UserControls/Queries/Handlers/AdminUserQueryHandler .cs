using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Features.Admin.UserControls.Queries.Models;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Queries.Handlers
{
    public class AdminUserQueryHandler :
        IRequestHandler<GetUsersQuery, PaginatedResult<UserListItemDTO>>,
        IRequestHandler<GetUserByIdQuery, Response<UserDetailsDTO>>
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserQueryHandler(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        public async Task<PaginatedResult<UserListItemDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            return await _adminUserService.GetUsersAsync(request.Search, request.PageNumber, request.PageSize);
        }

        public async Task<Response<UserDetailsDTO>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _adminUserService.GetUserByIdAsync(request.Id);
        }
    }
}
