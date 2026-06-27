using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Features.Notifications;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly INotificationService _notificationService;
        private readonly ILogger<AddCommentCommandHandler> _logger;

        public AddCommentCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            ILogger<AddCommentCommandHandler> logger)
            => (_uow, _currentUser, _notificationService, _logger) =
                (uow, currentUser, notificationService, logger);

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
            if (user is not null)
                comment.User = user;

            var dto = comment.Adapt<CommentDto>();
            dto.IsOwner = true;

            if (template.CreatorUserId != _currentUser.UserId)
            {
                var actorName = user?.Name ?? "Someone";
                await NotificationDispatchHelper.TrySendAsync(
                    _notificationService,
                    _logger,
                    template.CreatorUserId,
                    "New comment",
                    $"{actorName} commented on your template \"{template.Name}\".",
                    NotificationType.TemplateCommented);
            }

            return Created(dto);
        }
    }
}
