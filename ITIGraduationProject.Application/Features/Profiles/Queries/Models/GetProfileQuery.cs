using ITIGraduationProject.Application.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ITIGraduationProject.Application.DTOS.Profiles;

namespace ITIGraduationProject.Application.Features.Profiles.Queries.Models
{
    public class GetProfileQuery : IRequest<Response<ProfileDTO>>
    {
        public string UserId { get; set; }

    }
}
