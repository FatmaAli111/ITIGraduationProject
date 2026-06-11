using ITIGraduationProject.Application.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ITIGraduationProject.Application.Features.Profiles.Queries.GetProfile
{
    public class GetProfileQuery : IRequest<Response<ProfileDTO>>
    {
        public string UserId { get; set; }
    }
}
