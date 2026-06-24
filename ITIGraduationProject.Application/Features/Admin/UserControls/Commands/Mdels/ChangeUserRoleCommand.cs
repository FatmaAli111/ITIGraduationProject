using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels
{
    public class ChangeUserRoleCommand : IRequest<Response<string>>
    {
        public Guid Id { get; set; }
        public string NewRole { get; set; }
    }
}
