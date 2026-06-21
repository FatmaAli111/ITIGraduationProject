using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class DeleteCommentCommandHandler
        : ResponseHandler,
          IRequestHandler<DeleteCommentCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteCommentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<string>> Handle(
            DeleteCommentCommand cmd, CancellationToken ct)
        {
            var comment = await _uow.CommunityInteractions.GetByIdAsync(cmd.Id);

            if (comment is null || comment.IsDeleted || comment.InteractionType != InteractionType.Comment)
                return NotFound<string>("Comment not found");

            if (comment.UserId != _currentUser.UserId)
                return Unauthorized<string>();

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            _uow.CommunityInteractions.Update(comment);
            await _uow.SaveChangesAsync();

            return Deleted<string>();
        }
    }
}
