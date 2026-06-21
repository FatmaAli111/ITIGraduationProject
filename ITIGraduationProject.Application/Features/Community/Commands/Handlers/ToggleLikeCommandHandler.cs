using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public ToggleLikeCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            INotificationService notificationService)
            => (_uow, _currentUser, _notificationService) = (uow, currentUser, notificationService);

        public async Task<Response<LikeStatusDto>> Handle(
            ToggleLikeCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.TemplateId);
            if (template is null || template.IsDeleted)
                return NotFound<LikeStatusDto>("Template not found");

            var existingLike = await _uow.CommunityInteractions
                .GetTableAsTracking()
                .FirstOrDefaultAsync(
                    ci => ci.UserId == _currentUser.UserId
                       && ci.TemplateId == cmd.TemplateId
                       && ci.InteractionType == InteractionType.Like,
                    ct);

            if (existingLike is not null)
            {
                existingLike.IsDeleted = true;
                existingLike.DeletedAt = DateTime.UtcNow;
                _uow.CommunityInteractions.Update(existingLike);
                template.LikesCount = Math.Max(0, template.LikesCount - 1);
                _uow.Templates.Update(template);
                await _uow.SaveChangesAsync();

                return Success(new LikeStatusDto { Liked = false, Count = template.LikesCount });
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
                await CommunityNotificationHelper.TrySendAsync(
                    _notificationService,
                    template.CreatorUserId,
                    "New like",
                    $"{actorName} liked your template \"{template.Name}\".",
                    NotificationType.TemplateLiked);
            }

            return Success(new LikeStatusDto { Liked = true, Count = template.LikesCount });
        }
    }
}
