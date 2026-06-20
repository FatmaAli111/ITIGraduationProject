using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Templates.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Commands.Handlers
{
    public class DeleteTemplateCommandHandler
    : ResponseHandler,
      IRequestHandler<DeleteTemplateCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<string>> Handle(
            DeleteTemplateCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.Id);

            if (template is null || template.IsDeleted)
                return NotFound<string>("Template not found");

            if (template.CreatorUserId != _currentUser.UserId)
                return Unauthorized<string>();

            // Soft delete — EF query filter handles the rest
            template.IsDeleted = true;
            template.DeletedAt = DateTime.UtcNow;

            _uow.Templates.Update(template);
            await _uow.SaveChangesAsync();

            return Deleted<string>();
        }
    }
}
