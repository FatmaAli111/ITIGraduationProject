using MediatR;
using ITIGraduationProject.Application.Bases;
using System;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Application.DTOS.AiChat;

namespace ITIGraduationProject.Application.Features.AiChat.Queries.Models
{
    public class GetChatSessionHistoryQuery : IRequest<Response<ChatHistoryDto>>
    {
        public Guid SessionId { get; set; }
        public Guid RequestingUserId { get; set; }
    }
}