using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Queries.Models
{
    public class GetUsersQuery : IRequest<PaginatedResult<UserListItemDTO>>
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
