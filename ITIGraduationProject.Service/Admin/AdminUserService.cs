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
using Microsoft.Extensions.Logging;
using ITIGraduationProject.Domain.Enums;
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
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AdminUserService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Response<string>> InviteUserAsync(InviteUserRequestDTO request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest<string>("Email already exists.");

            if (request.Role != Roles.Admin && request.Role != Roles.Printer)
                return BadRequest<string>("Invalid role for invitation.");

            if (request.Role == Roles.Printer &&
                (!HasValidFabricFlags(request.SupportedFabrics) ||
                 !HasValidPrintMethodFlags(request.SupportedPrintMethods)))
            {
                return BadRequest<string>(
                    "At least one valid fabric and print method is required for printers.");
            }

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
                await CleanupInvitedUserAsync(domainUser);
                return BadRequest<string>(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(applicationUser, request.Role);
            if (!roleResult.Succeeded)
            {
                await CleanupInvitedUserAsync(domainUser, applicationUser);
                return BadRequest<string>(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            try
            {
                await SendInvitationEmailAsync(applicationUser, request.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invitation email to {Email}", request.Email);
                await CleanupInvitedUserAsync(domainUser, applicationUser);
                return BadRequest<string>(
                    "Invitation email could not be sent. No user was created.");
            }

            return Success<string>(applicationUser.Id.ToString(), "Invitation sent successfully.");
        }
        //var invitationLink =
        //   $"{_configuration.GetSection("ClientSettings:ClientBaseUrl").Value}/reset-password?email={applicationUser.Email}&token={encodedToken}";

        public async Task<Response<string>> ResendInvitationAsync(Guid id)
        {
            var applicationUser = await _userManager.FindByIdAsync(id.ToString());
            if (applicationUser == null || string.IsNullOrWhiteSpace(applicationUser.Email))
                return NotFound<string>("User not found.");

            if (applicationUser.EmailConfirmed)
                return BadRequest<string>("This user has already accepted the invitation.");

            var roles = await _userManager.GetRolesAsync(applicationUser);
            var role = roles.FirstOrDefault();
            if (role != Roles.Admin && role != Roles.Printer)
                return BadRequest<string>("Only Admin and Printer invitations can be resent.");

            try
            {
                await SendInvitationEmailAsync(applicationUser, role);
                return new Response<string>
                {
                    StatusCode = HttpStatusCode.OK,
                    Succeeded = true,
                    Message = "Invitation email resent successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend invitation email to {Email}", applicationUser.Email);
                return BadRequest<string>("Invitation email could not be sent.");
            }
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
                    IsActive = user.IsActive,
                    EmailConfirmed = appUser?.EmailConfirmed ?? false
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

        public async Task<Response<string>> ChangeUserRoleAsync(Guid id, string newRole)
        {
            var validRoles = new[] { Roles.Admin, Roles.User, Roles.Printer };
            if (!validRoles.Contains(newRole))
                return BadRequest<string>("Invalid role.");

            var applicationUser = await _userManager.FindByIdAsync(id.ToString());
            if (applicationUser == null)
                return NotFound<string>("User not found.");

            var currentRoles = await _userManager.GetRolesAsync(applicationUser);
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(applicationUser, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest<string>(string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            var addResult = await _userManager.AddToRoleAsync(applicationUser, newRole);
            if (!addResult.Succeeded)
            {
                if (currentRoles.Count > 0)
                    await _userManager.AddToRolesAsync(applicationUser, currentRoles);

                return BadRequest<string>(string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }

            return Success<string>(null, "User role updated successfully.");
        }

        public async Task<string> GetUserRoleAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return string.Empty;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        private static bool HasValidFabricFlags(FabricType? value)
        {
            const FabricType allowed = FabricType.Cotton | FabricType.Polyester |
                                       FabricType.Wool | FabricType.Silk | FabricType.Linen;
            return value.HasValue && value.Value != 0 && (value.Value & ~allowed) == 0;
        }

        private async Task SendInvitationEmailAsync(ApplicationUser applicationUser, string role)
        {
            if (string.IsNullOrWhiteSpace(applicationUser.Email))
                throw new InvalidOperationException("Invitation email address is missing.");

            var clientBaseUrl = _configuration["ClientSettings:ClientBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(clientBaseUrl))
                throw new InvalidOperationException("Client base URL is not configured.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            var encodedToken = WebUtility.UrlEncode(token);
            var invitationLink = $"{clientBaseUrl}/reset-password?email={applicationUser.Email}&token={encodedToken}";
            var body = $"<h3>You've been invited as {role}!</h3>" +
                       $"<p>Click <a href='{invitationLink}'>here</a> to set your password and activate your account.</p>";

            await _emailService.SendEmailAsync(
                applicationUser.Email,
                $"Invitation - {role}",
                body);
        }

        private static bool HasValidPrintMethodFlags(PrintMethodType? value)
        {
            const PrintMethodType allowed = PrintMethodType.DirectToGarment |
                                            PrintMethodType.ScreenPrinting |
                                            PrintMethodType.HeatTransfer |
                                            PrintMethodType.Sublimation |
                                            PrintMethodType.Embroidery;
            return value.HasValue && value.Value != 0 && (value.Value & ~allowed) == 0;
        }

        private async Task CleanupInvitedUserAsync(
            User domainUser,
            ApplicationUser? applicationUser = null)
        {
            try
            {
                if (applicationUser is not null)
                    await _userManager.DeleteAsync(applicationUser);

                _unitOfWork.Users.Delete(domainUser);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(
                    cleanupException,
                    "Failed to roll back invited user {UserId}",
                    domainUser.Id);
            }
        }
    }
}