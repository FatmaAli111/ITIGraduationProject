using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class RegisterCommand : IRequest<Response<string>>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
