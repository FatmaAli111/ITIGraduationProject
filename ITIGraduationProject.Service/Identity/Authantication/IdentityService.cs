using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
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

namespace ITIGraduationProject.Service.Identity.Authantication
{
        public class IdentityService : IIdentityService
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IEmailService _emailService;
            private readonly IConfiguration _configuration;

        public IdentityService(
                UserManager<ApplicationUser> userManager,
                IUnitOfWork unitOfWork, IEmailService emailService,IConfiguration configuration)
            {
                _userManager = userManager;
                _unitOfWork = unitOfWork;
           _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return "Email already exists.";

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
                return string.Join(", ", result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(applicationUser, Roles.User);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink = $"{_configuration.GetSection("AppSettings:ClientBaseUrl").Value}/api/identity/confirm-email?userId={applicationUser.Id}&token={encodedToken}";

            var body = $"<h3>Welcome {request.Name}!</h3><p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";

            await _emailService.SendEmailAsync(request.Email, "Confirm your email", body);

            return "Registration successful. Please check your email to confirm your account.";
        }
        public async Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
                return new IdentityResultDto { Succeeded = false, Message = "Invalid user id." };

            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser == null)
                return new IdentityResultDto { Succeeded = false, Message = "User not found." };

            if (applicationUser.EmailConfirmed)
                return new IdentityResultDto { Succeeded = true, Message = "Email already confirmed." };

            var decodedToken = WebUtility.UrlDecode(token); 

            var result = await _userManager.ConfirmEmailAsync(applicationUser, decodedToken);
            if (!result.Succeeded)
                return new IdentityResultDto
                {
                    Succeeded = false,
                    Message = "Email confirmation failed.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };

            var domainUser = await _unitOfWork.Users.GetByIdAsync(parsedUserId);
            if (domainUser != null)
            {
                domainUser.IsActive = true;
                _unitOfWork.Users.Update(domainUser);
                await _unitOfWork.SaveChangesAsync();
            }

            return new IdentityResultDto { Succeeded = true, Message = "Email confirmed successfully." };
        }
    }
    }

