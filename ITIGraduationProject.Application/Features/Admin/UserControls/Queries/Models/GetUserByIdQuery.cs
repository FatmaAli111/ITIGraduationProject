using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Queries.Models
{
    public class GetUserByIdQuery : IRequest<Response<UserDetailsDTO>>
    {
        public Guid Id { get; set; }
    }
}
