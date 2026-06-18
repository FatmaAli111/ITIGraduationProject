using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Service.Identity.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public IdentityService(
                UserManager<ApplicationUser> userManager,
                IUnitOfWork unitOfWork, IEmailService emailService,
                IConfiguration configuration, IJwtService JwtService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _jwtService = JwtService;
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
                IsActive = false,
                CurrentPointsBalance = 0,
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

            var confirmationLink = $"{_configuration.GetSection("AppSettings:ClientBaseUrl").Value}/api/identity/confirm-email?userId={applicationUser.Id}&token={encodedToken}";

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

            var decodedToken = WebUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(applicationUser, decodedToken);
            if (!result.Succeeded)
                return BadRequest<string>("Email confirmation failed.");

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
                return Unauthorized<LoginResponseDTO>();

            if (!applicationUser.EmailConfirmed)
                return BadRequest<LoginResponseDTO>("Please confirm your email first.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(applicationUser, request.Password);
            if (!isPasswordValid)
                return Unauthorized<LoginResponseDTO>();

            var domainUser = await _unitOfWork.Users.GetByIdAsync(applicationUser.Id);
            if (domainUser == null || !domainUser.IsActive)
                return BadRequest<LoginResponseDTO>("Account is not active.");

            var roles = await _userManager.GetRolesAsync(applicationUser);

            var (accessToken, expiresAt) = _jwtService.GenerateToken(
                applicationUser.Id.ToString(),
                applicationUser.Email,
                roles.ToList());

            var response = new LoginResponseDTO
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                Email = applicationUser.Email,
                Name = domainUser.Name,
                Roles = roles.ToList()
            };

            return Success(response, "Login successful.");
        }
    }
}