using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Application.Wrapers.Templates.CQRS;
using ITIGraduationProject.Application.Wrapers.Templates.DTOs;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class GetMyTemplatesQueryHandler
    : ResponseHandler,
      IRequestHandler<GetMyTemplatesQuery, Response<PaginatedResult<TemplateDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public GetMyTemplatesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<PaginatedResult<TemplateDto>>> Handle(
            GetMyTemplatesQuery request, CancellationToken ct)
        {
            var result = await _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.CreatorUserId == _currentUser.UserId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ProjectToType<TemplateDto>()
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Success(result);
        }
    }
}
