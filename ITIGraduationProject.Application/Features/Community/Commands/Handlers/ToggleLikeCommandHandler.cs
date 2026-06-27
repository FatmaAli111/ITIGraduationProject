using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Features.Notifications;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class ToggleLikeCommandHandler
        : ResponseHandler,
          IRequestHandler<ToggleLikeCommand, Response<LikeStatusDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ToggleLikeCommandHandler> _logger;

        public ToggleLikeCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            ILogger<ToggleLikeCommandHandler> logger)
            => (_uow, _currentUser, _notificationService, _logger) =
                (uow, currentUser, notificationService, logger);

        public async Task<Response<LikeStatusDto>> Handle(
            ToggleLikeCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.TemplateId);
            if (template is null || template.IsDeleted)
                return NotFound<LikeStatusDto>("Template not found");

            var existingLike = await _uow.CommunityInteractions
                .GetTableAsTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    ci => ci.UserId == _currentUser.UserId
                       && ci.TemplateId == cmd.TemplateId
                       && ci.InteractionType == InteractionType.Like,
                    ct);

            if (existingLike is not null)
            {
                if (!existingLike.IsDeleted)
                {
                    existingLike.IsDeleted = true;
                    existingLike.DeletedAt = DateTime.UtcNow;
                    template.LikesCount = Math.Max(0, template.LikesCount - 1);
                }
                else
                {
                    existingLike.IsDeleted = false;
                    existingLike.DeletedAt = null;
                    template.LikesCount++;
                }
                _uow.CommunityInteractions.Update(existingLike);
                _uow.Templates.Update(template);
                await _uow.SaveChangesAsync();

                return Success(new LikeStatusDto { Liked = !existingLike.IsDeleted, Count = template.LikesCount });
            }

            var like = new CommunityInteraction
            {
                UserId = _currentUser.UserId,
                TemplateId = cmd.TemplateId,
                InteractionType = InteractionType.Like
            };

            await _uow.CommunityInteractions.AddAsync(like);
            template.LikesCount++;
            _uow.Templates.Update(template);
            await _uow.SaveChangesAsync();

            if (template.CreatorUserId != _currentUser.UserId)
            {
                var actor = await _uow.Users.GetByIdAsync(_currentUser.UserId);
                var actorName = actor?.Name ?? "Someone";
                await NotificationDispatchHelper.TrySendAsync(
                    _notificationService,
                    _logger,
                    template.CreatorUserId,
                    "New like",
                    $"{actorName} liked your template \"{template.Name}\".",
                    NotificationType.TemplateLiked);
            }

            return Success(new LikeStatusDto { Liked = true, Count = template.LikesCount });
        }
    }
}
