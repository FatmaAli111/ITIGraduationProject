using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class LogoutCommand : IRequest<Response<string>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
