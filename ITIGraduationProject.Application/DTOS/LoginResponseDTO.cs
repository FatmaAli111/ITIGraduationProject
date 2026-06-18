using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
