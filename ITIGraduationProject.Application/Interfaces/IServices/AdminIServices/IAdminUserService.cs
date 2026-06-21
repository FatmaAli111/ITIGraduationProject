using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Wrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.AdminIServices
{
    public interface IAdminUserService
    {
        Task<Response<string>> InviteUserAsync(InviteUserRequestDTO request);
        Task<PaginatedResult<UserListItemDTO>> GetUsersAsync(string? search, int pageNumber, int pageSize);
        Task<Response<UserDetailsDTO>> GetUserByIdAsync(Guid id);
        Task<Response<string>> UpdateUserAsync(Guid id, UpdateUserRequestDTO request);
        Task<Response<string>> ChangeUserStatusAsync(Guid id, bool isActive);
    }
}
