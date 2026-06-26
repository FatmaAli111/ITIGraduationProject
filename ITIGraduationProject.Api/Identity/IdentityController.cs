using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Models;
using ITIGraduationProject.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITIGraduationProject.Api.IdentityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator _mediatr;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityController(IMediator mediatr,SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
        {
            _mediatr = mediatr;
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterCommand requestDTO)
        {
            var Result =await _mediatr.Send(requestDTO);
            return Ok(Result);
        }
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command)
        {
            var result = await _mediatr.Send(command);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _mediatr.Send(command);
            return Ok(result);
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand request)
        {
            var result =await _mediatr.Send(request);
            return Ok(result);

        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command)
        {
            var result = await _mediatr.Send(command);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAllDevices()
        {
            var result = await _mediatr.Send(new LogoutAllDevicesCommand());

            return Ok(result);
        }
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(
      ForgetPasswordCommand command)
        {
            var result = await _mediatr.Send(command);

            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordCommand command)
        {
            var result = await _mediatr.Send(command);

            return Ok(result);
        }

        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider)
        {
            provider = provider switch
            {
                "google" => "Google",
                _ => provider
            };

            var redirectUrl =
                              "/api/Identity/external-login-callback";
            var properties =
                _signInManager.ConfigureExternalAuthenticationProperties(
                    provider,
                    redirectUrl);

            return Challenge(properties, provider);
        }
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var response = await _mediatr.Send(new ExternalLoginCommand());

            if (!response.Succeeded)
            {
                return Redirect(
                    $"http://localhost:4200/auth?error={Uri.EscapeDataString(response.Message ?? "Google login failed")}");
            }

            var data = response.Data;

            var redirectUrl =
                $"http://localhost:4200/auth/google-callback" +
                $"?name={Uri.EscapeDataString(data.Name)}" +
                $"&expiresAt={Uri.EscapeDataString(data.ExpiresAt.ToString("O"))}" +
                $"&email={Uri.EscapeDataString(data.Email)}" +
                $"&accessToken={Uri.EscapeDataString(data.AccessToken)}" +
                $"&refreshToken={Uri.EscapeDataString(data.RefreshToken)}" +
                $"&roles={Uri.EscapeDataString(string.Join(",", data.Roles ?? new List<string>()))}";

            return Redirect(redirectUrl);
        }
    }
}
