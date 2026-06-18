using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Templates.CQRS;
using ITIGraduationProject.Application.Wrapers.Templates.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class GetTemplateByIdQueryHandler
    : ResponseHandler,
      IRequestHandler<GetTemplateByIdQuery, Response<TemplateDetailDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetTemplateByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<TemplateDetailDto>> Handle(
            GetTemplateByIdQuery request, CancellationToken ct)
        {
            var template = await _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.Id == request.Id && !t.IsDeleted)
                .ProjectToType<TemplateDetailDto>()
                .FirstOrDefaultAsync(ct);

            if (template is null)
                return NotFound<TemplateDetailDto>("Template not found");

            return Success(template);
        }
    }
}
