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
        Task<string> RegisterAsync(RegisterRequestDTO request);
        Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token);

    }
}
