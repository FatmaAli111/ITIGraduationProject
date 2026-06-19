using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class ForgetPasswordCommand
     : IRequest<Response<string>>
    {
        public string Email { get; set; } = string.Empty;
    }
}
