using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class LoginCommand : IRequest<Response<LoginResponseDTO>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
