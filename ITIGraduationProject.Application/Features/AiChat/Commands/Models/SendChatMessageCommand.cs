using MediatR;
using ITIGraduationProject.Application.Bases;
using System;

namespace ITIGraduationProject.Application.Features.AiChat.Commands.Models
{
    public class SendChatMessageCommand : IRequest<Response<string>>
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string MessageText { get; set; } = string.Empty;
    }
}