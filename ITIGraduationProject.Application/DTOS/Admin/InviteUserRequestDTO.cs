using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.Admin
{
    public class InviteUserRequestDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public FabricType? SupportedFabrics { get; set; }
        public PrintMethodType? SupportedPrintMethods { get; set; }
    }
}
