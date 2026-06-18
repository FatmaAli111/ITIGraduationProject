using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Models
{
    public class ConfirmEmailCommand:IRequest<Response<string>>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
