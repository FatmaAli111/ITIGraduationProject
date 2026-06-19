using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.IdentityServices
{
   
        public interface IIdentityService
        {
            Task<Response<string>> RegisterAsync(RegisterRequestDTO request);
            Task<Response<string>> ConfirmEmailAsync(string userId, string token);
            Task<Response<LoginResponseDTO>> LoginAsync(LoginRequestDTO request);
            Task<Response<LoginResponseDTO>> RefreshTokenAsync(string refreshToken);
            Task<Response<string>> LogoutAsync(string refreshToken);
            Task<Response<string>> LogoutAllDevicesAsync();
            Task<Response<string>> ForgetPasswordAsync(string email);
           Task<Response<string>> ResetPasswordAsync(string email, string token,string newPassword);
           Task<Response<ExternalLoginResponseDTO>> ExternalLoginAsync();
    }
    
}
