using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.IdentityServices
{
    public interface IJwtService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, List<string> roles);
    }
}
