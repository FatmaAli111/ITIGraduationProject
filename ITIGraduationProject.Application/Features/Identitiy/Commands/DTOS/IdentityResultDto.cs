using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS
{
    public class IdentityResultDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? UserId { get; set; }
    }
}
