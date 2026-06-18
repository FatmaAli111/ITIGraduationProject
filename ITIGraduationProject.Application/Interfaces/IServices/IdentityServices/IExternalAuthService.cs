using ITIGraduationProject.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.IdentityServices
{
    public interface IExternalAuthService
    {
        Task<ExternalUserInfoDTO> GetUserInfoAsync(
            string provider,
            string token);
    }
}
