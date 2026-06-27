using ITIGraduationProject.Application.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ITIGraduationProject.Application.Features.Profiles.Commands.Models
{
    public class UpdateProfileCommand : IRequest<Response<string>>
    {
        public string? UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string? NewPassword { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
