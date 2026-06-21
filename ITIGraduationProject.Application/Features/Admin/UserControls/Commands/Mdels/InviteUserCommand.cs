using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels
{
    public class InviteUserCommand : IRequest<Response<string>>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public FabricType? SupportedFabrics { get; set; }
        public PrintMethodType? SupportedPrintMethods { get; set; }
    }
}
