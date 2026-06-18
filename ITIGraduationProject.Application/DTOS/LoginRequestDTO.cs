using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS
{
    public class LoginRequestDTO : IRequest<Response<LoginResponseDTO>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
