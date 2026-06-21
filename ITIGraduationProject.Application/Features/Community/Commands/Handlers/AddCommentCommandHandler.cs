using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class AddCommentCommandHandler
        : ResponseHandler,
          IRequestHandler<AddCommentCommand, Response<CommentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public AddCommentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<CommentDto>> Handle(
            AddCommentCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.TemplateId);
            if (template is null || template.IsDeleted)
                return NotFound<CommentDto>("Template not found");

            var comment = new CommunityInteraction
            {
                UserId = _currentUser.UserId,
                TemplateId = cmd.TemplateId,
                InteractionType = InteractionType.Comment,
                Content = cmd.Content
            };

            await _uow.CommunityInteractions.AddAsync(comment);
            await _uow.SaveChangesAsync();

            var user = await _uow.Users.GetByIdAsync(_currentUser.UserId);
            var dto = comment.Adapt<CommentDto>();
            dto.UserName = user?.Name ?? string.Empty;
            dto.UserProfileImageUrl = user?.ProfileImageUrl;

            return Created(dto);
        }
    }
}
