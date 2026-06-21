using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Service.Admin
{
    public class AdminUserService : ResponseHandler, IAdminUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AdminUserService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<Response<string>> InviteUserAsync(InviteUserRequestDTO request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest<string>("Email already exists.");

            if (request.Role != Roles.Admin && request.Role != Roles.Printer)
                return BadRequest<string>("Invalid role for invitation.");

            var newId = Guid.NewGuid();

            var domainUser = new User
            {
                Id = newId,
                Name = request.Name,
                IsActive = false,
                CurrentPointsBalance = 0,
                UserPreferences = new UserPreferences(),
                Cart = new Cart()
            };

            if (request.Role == Roles.Printer)
            {
                domainUser.PrinterProfile = new PrinterProfile
                {
                    UserId = newId,
                    SupportedFabrics = request.SupportedFabrics.Value,
                    SupportedPrintMethods = request.SupportedPrintMethods.Value,
                    IsActive = true
                };
            }

            await _unitOfWork.Users.AddAsync(domainUser);
            await _unitOfWork.SaveChangesAsync();

            var applicationUser = new ApplicationUser
            {
                Id = newId,
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = false
            };

            var randomPassword = Guid.NewGuid().ToString("N") + "Aa1!";
            var result = await _userManager.CreateAsync(applicationUser, randomPassword);

            if (!result.Succeeded)
            {
                _unitOfWork.Users.Delete(domainUser);
                await _unitOfWork.SaveChangesAsync();
                return BadRequest<string>(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(applicationUser, request.Role);

            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            var encodedToken = WebUtility.UrlEncode(token);

            var invitationLink = $"{_configuration["AppSettings:ClientBaseUrl"]}/accept-invitation?userId={applicationUser.Id}&token={encodedToken}";

            var body = $"<h3>You've been invited as {request.Role}!</h3>" +
                        $"<p>Click <a href='{invitationLink}'>here</a> to set your password and activate your account.</p>";

            await _emailService.SendEmailAsync(request.Email, $"Invitation - {request.Role}", body);

            return Success<string>(applicationUser.Id.ToString(), "Invitation sent successfully.");
        }

        public async Task<PaginatedResult<UserListItemDTO>> GetUsersAsync(string? search, int pageNumber, int pageSize)
        {
            var query = Search.ApplySearch(_unitOfWork.Users.GetTableNoTracking(), search);

            var paginatedUsers = await query.ToPaginatedListAsync(pageNumber, pageSize);

            var userDtos = new List<UserListItemDTO>();

            foreach (var user in paginatedUsers.Data)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
                var roles = appUser != null ? await _userManager.GetRolesAsync(appUser) : new List<string>();

                userDtos.Add(new UserListItemDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = appUser?.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "",
                    JoinedAt = user.CreatedAt,
                    IsActive = user.IsActive
                });
            }

            return PaginatedResult<UserListItemDTO>.Success(
                userDtos,
                paginatedUsers.TotalCount,
                paginatedUsers.CurrentPage,
                paginatedUsers.PageSize);
        }
        public async Task<Response<UserDetailsDTO>> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return NotFound<UserDetailsDTO>("User not found.");

            var appUser = await _userManager.FindByIdAsync(id.ToString());
            var roles = appUser != null ? await _userManager.GetRolesAsync(appUser) : new List<string>();

            var dto = new UserDetailsDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = appUser?.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                IsActive = user.IsActive,
                JoinedAt = user.CreatedAt,
                CurrentPointsBalance = user.CurrentPointsBalance
            };

            return Success(dto);
        }

        public async Task<Response<string>> UpdateUserAsync(Guid id, UpdateUserRequestDTO request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return NotFound<string>("User not found.");

            user.Name = request.Name;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Success<string>(null, "User updated successfully.");
        }

        public async Task<Response<string>> ChangeUserStatusAsync(Guid id, bool isActive)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                return NotFound<string>("User not found.");

            user.IsActive = isActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Success<string>(null, isActive ? "User activated." : "User deactivated.");
        }
    }
}
