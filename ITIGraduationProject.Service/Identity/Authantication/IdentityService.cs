using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Service.Identity.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Service.Identity.Authantication
{
    public class IdentityService : ResponseHandler, IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(
                UserManager<ApplicationUser> userManager,
                IUnitOfWork unitOfWork, IEmailService emailService,
                IConfiguration configuration, IJwtService JwtService,
                  IHttpContextAccessor httpContextAccessor
            , SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _jwtService = JwtService;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest<string>("Email already exists.");

            var newId = Guid.NewGuid();

            var domainUser = new User
            {
                Id = newId,
                Name = request.Name,
                Email = request.Email,       
                UserName = request.Email,
                IsActive = false,
                CurrentPointsBalance = 0,
                OnboardingCompleted = false,
                UserPreferences = new UserPreferences(),
                Cart = new Cart()
            };

            await _unitOfWork.Users.AddAsync(domainUser);
            await _unitOfWork.SaveChangesAsync();

            var applicationUser = new ApplicationUser
            {
                Id = newId,
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(applicationUser, request.Password);
            if (!result.Succeeded)
            {
                _unitOfWork.Users.Delete(domainUser);
                await _unitOfWork.SaveChangesAsync();
                return BadRequest<string>(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(applicationUser, Roles.User);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink = $"{_configuration.GetSection("ClientSettings:ClientBaseUrl").Value}/confirm-mail?userId={applicationUser.Id}&token={encodedToken}";

            var body = $"<h3>Welcome {request.Name}!</h3><p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";

            await _emailService.SendEmailAsync(request.Email, "Confirm your email", body);

            return Success<string>(applicationUser.Id.ToString(),
                "Registration successful. Please check your email to confirm your account.");
        }

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
                return BadRequest<string>("Invalid user id.");

            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser == null)
                return NotFound<string>("User not found.");

            if (applicationUser.EmailConfirmed)
                return Success<string>(null, "Email already confirmed.");

            var result = await _userManager.ConfirmEmailAsync(applicationUser, token);

            if (!result.Succeeded)
                return BadRequest<string>(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            var domainUser = await _unitOfWork.Users.GetByIdAsync(parsedUserId);

            if (domainUser != null)
            {
                domainUser.IsActive = true;
                _unitOfWork.Users.Update(domainUser);
                await _unitOfWork.SaveChangesAsync();
            }

            return Success<string>(null, "Email confirmed successfully.");
        }
        public async Task<Response<LoginResponseDTO>> LoginAsync(LoginRequestDTO request)
        {
            var applicationUser = await _userManager.FindByEmailAsync(request.Email);
            if (applicationUser == null)
                return BadRequest<LoginResponseDTO>("This Email Doesn't Exist Please Register First");

            if (!applicationUser.EmailConfirmed)
                return BadRequest<LoginResponseDTO>("Please confirm your email first.");

            if (await _userManager.IsLockedOutAsync(applicationUser))
                return BadRequest<LoginResponseDTO>(
                    $"Account locked. Try again after {applicationUser.LockoutEnd?.ToLocalTime():HH:mm}.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(applicationUser, request.Password);
            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(applicationUser);
                applicationUser = await _userManager.FindByIdAsync(applicationUser.Id.ToString());

                if (await _userManager.IsLockedOutAsync(applicationUser))
                    return BadRequest<LoginResponseDTO>("Account locked due to multiple failed attempts. Try again after 15 minutes.");

                return BadRequest<LoginResponseDTO>("Email or Password Wrong");
            }

            await _userManager.ResetAccessFailedCountAsync(applicationUser);

            var domainUser = await _unitOfWork.Users.GetByIdAsync(applicationUser.Id);
            if (domainUser == null || !domainUser.IsActive)
                return BadRequest<LoginResponseDTO>("Account is not active.");

            var roles = await _userManager.GetRolesAsync(applicationUser);

            var (accessToken, expiresAt) = _jwtService.GenerateToken(
                applicationUser.Id.ToString(),
                applicationUser.Email,
                roles.ToList());

            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = applicationUser.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var response = new LoginResponseDTO
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                RefreshToken = refreshTokenValue,
                Email = applicationUser.Email,
                Name = domainUser.Name,
                Roles = roles.ToList(),
                OnboardingCompleted = domainUser.OnboardingCompleted
            };

            return Success(response, "Login successful.");
        }
        public async Task<Response<LoginResponseDTO>> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _unitOfWork.RefreshTokens
                .GetByTokenAsync(refreshToken);

            if (storedToken == null)
                return Unauthorized<LoginResponseDTO>();

            if (storedToken.IsRevoked)
                return Unauthorized<LoginResponseDTO>();

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
                return Unauthorized<LoginResponseDTO>();

            var applicationUser = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

            if (applicationUser == null)
                return Unauthorized<LoginResponseDTO>();

            var roles = await _userManager.GetRolesAsync(applicationUser);

            var (accessToken, expiresAt) = _jwtService.GenerateToken(
                applicationUser.Id.ToString(),
                applicationUser.Email!,
                roles.ToList());

            // Rotation
            storedToken.IsRevoked = true;

            var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = applicationUser.Id,
                Token = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _unitOfWork.RefreshTokens.Update(storedToken);
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);

            await _unitOfWork.SaveChangesAsync();

            var domainUser = await _unitOfWork.Users.GetByIdAsync(applicationUser.Id);

            return Success(new LoginResponseDTO
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                RefreshToken = newRefreshTokenValue,
                Email = applicationUser.Email,
                Name = domainUser?.Name,
                Roles = roles.ToList(),
                OnboardingCompleted = domainUser?.OnboardingCompleted ?? false
            });
        }
        public async Task<Response<string>> LogoutAsync(string refreshToken)
        {
            var storedToken = await _unitOfWork.RefreshTokens
                .GetByTokenAsync(refreshToken);

            if (storedToken == null)
                return Unauthorized<string>();

            storedToken.IsRevoked = true;

            _unitOfWork.RefreshTokens.Update(storedToken);

            await _unitOfWork.SaveChangesAsync();

            return Success("Logged out successfully");
        }
        public async Task<Response<string>> LogoutAllDevicesAsync()
        {
            var userId = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (userId == null)
                return Unauthorized<string>();


            var tokens = await _unitOfWork.RefreshTokens
                .GetUserTokensAsync(Guid.Parse(userId));


            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _unitOfWork.SaveChangesAsync();


            return Success("Logged out from all devices successfully");
        }
        public async Task<Response<string>> ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return BadRequest<string>("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebUtility.UrlEncode(token);

            var resetLink =
                $"{_configuration.GetSection("ClientSettings:ClientBaseUrl").Value}/reset-password?email={user.Email}&token={encodedToken}";

            var body =
                $"<h3>Password Reset</h3>" +
                $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Reset Password",
                body);

            return Success<string>(
                null,
                "Password reset link sent successfully.");
        }
        public async Task<Response<string>> ResetPasswordAsync(
        string email,
        string token,
        string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound<string>("User not found.");

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

            if (!result.Succeeded)
                return BadRequest<string>(
                    string.Join(", ", result.Errors.Select(x => x.Description)));

            return Success<string>(
                null,
                "Password reset successfully.");
        }

        public async Task<Response<string>> AcceptInvitationAsync(
            Guid userId,
            string token,
            string newPassword)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
            if (applicationUser == null)
                return NotFound<string>("Invitation user not found.");

            var domainUser = await _unitOfWork.Users.GetByIdAsync(userId);
            if (domainUser == null)
                return NotFound<string>("Invitation user profile not found.");

            if (applicationUser.EmailConfirmed && domainUser.IsActive)
                return BadRequest<string>("This invitation has already been accepted.");

            applicationUser.EmailConfirmed = true;
            var resetResult = await _userManager.ResetPasswordAsync(
                applicationUser,
                token,
                newPassword);

            if (!resetResult.Succeeded)
            {
                applicationUser.EmailConfirmed = false;
                return BadRequest<string>(
                    string.Join(", ", resetResult.Errors.Select(error => error.Description)));
            }

            domainUser.IsActive = true;
            _unitOfWork.Users.Update(domainUser);
            await _unitOfWork.SaveChangesAsync();

            return new Response<string>
            {
                StatusCode = HttpStatusCode.OK,
                Succeeded = true,
                Message = "Invitation accepted. You can now sign in."
            };
        }

        public async Task<Response<LoginResponseDTO>> ExternalLoginAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return BadRequest<LoginResponseDTO>("External login failed.");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return BadRequest<LoginResponseDTO>("Email not found from provider.");

            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            var applicationUser = await _userManager.FindByEmailAsync(email);

            if (applicationUser == null)
            {
                var newId = Guid.NewGuid();

                var domainUser = new User
                {
                    Id = newId,
                    Name = name ?? email.Split('@')[0],
                    Email = email,               
                    UserName = email,
                    IsActive = true,
                    CurrentPointsBalance = 0,
                    OnboardingCompleted = false,
                    UserPreferences = new UserPreferences(),
                    Cart = new Cart()
                };

                await _unitOfWork.Users.AddAsync(domainUser);

                applicationUser = new ApplicationUser
                {
                    Id = newId,
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(applicationUser);

                if (!result.Succeeded)
                {
                    _unitOfWork.Users.Delete(domainUser);
                    await _unitOfWork.SaveChangesAsync();

                    return BadRequest<LoginResponseDTO>(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(applicationUser, Roles.User);
            }

            var logins = await _userManager.GetLoginsAsync(applicationUser);

            if (!logins.Any(x => x.LoginProvider == info.LoginProvider))
            {
                await _userManager.AddLoginAsync(applicationUser, info);
            }

            var roles = await _userManager.GetRolesAsync(applicationUser);

            var (accessToken, expiresAt) = _jwtService.GenerateToken(
                applicationUser.Id.ToString(),
                applicationUser.Email,
                roles.ToList());

            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = applicationUser.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var domainUserForResponse = await _unitOfWork.Users.GetByIdAsync(applicationUser.Id);

            var response = new LoginResponseDTO
            {
                Name = applicationUser.UserName,
                Email = applicationUser.Email,
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,

                OnboardingCompleted = domainUserForResponse?.OnboardingCompleted ?? false,
                ExpiresAt = expiresAt,
                Roles = roles.ToList()
            };

            return Success(response, "External login successful.");
        }

        public async Task<Response<string>> SaveOnboardingAsync(Guid userId, SaveOnboardingDTO dto)
        {
            var domainUser = await _unitOfWork.Users.GetWithProfileCartAndPreferencesAsync(userId);
            if (domainUser == null)
                return NotFound<string>("User not found.");

            if (domainUser.UserPreferences != null)
            {
                domainUser.UserPreferences.FavoriteColors = dto.FavoriteColors;
                domainUser.UserPreferences.Interests = dto.Interests;
                domainUser.UserPreferences.DesignPreference = dto.DesignPreference;
            }

            domainUser.OnboardingCompleted = true;

            _unitOfWork.Users.Update(domainUser);
            await _unitOfWork.SaveChangesAsync();

            return Success<string>(null, "Onboarding completed successfully.");
        }
        public async Task<Response<UserPreferencesDTO>> GetOnboardingAsync(Guid userId)
        {
            var domainUser = await _unitOfWork.Users.GetWithProfileCartAndPreferencesAsync(userId);
            if (domainUser == null)
                return NotFound<UserPreferencesDTO>("User not found.");

            var prefs = domainUser.UserPreferences;
            var dto = new UserPreferencesDTO
            {
                FavoriteColors = prefs?.FavoriteColors ?? "",
                Interests = prefs?.Interests ?? "",
                DesignPreference = prefs?.DesignPreference ?? ""
            };

            return Success(dto, "Preferences retrieved successfully.");
        }
    }
}
