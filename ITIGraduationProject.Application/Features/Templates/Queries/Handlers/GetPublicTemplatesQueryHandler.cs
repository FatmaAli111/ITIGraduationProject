using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class GetPublicTemplatesQueryHandler
    : ResponseHandler,
      IRequestHandler<GetPublicTemplatesQuery, Response<PaginatedResult<TemplateDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetPublicTemplatesQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<PaginatedResult<TemplateDto>>> Handle(
            GetPublicTemplatesQuery request, CancellationToken ct)
        {
            var result = await _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.IsPublic && !t.IsDeleted)
                .OrderByDescending(t => t.LikesCount)
                .ProjectToType<TemplateDto>()
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Success(result);
        }
    }
}
