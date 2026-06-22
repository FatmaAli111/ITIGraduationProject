using MediatR;
using ITIGraduationProject.Application.Bases;
using System;
using ITIGraduationProject.Domain.Entities.AIAndModeration;

namespace ITIGraduationProject.Application.Features.AiChat.Queries.Models
{
    public class GetChatSessionHistoryQuery : IRequest<Response<AiChatSession>>
    {
        public Guid SessionId { get; set; }
    }
}