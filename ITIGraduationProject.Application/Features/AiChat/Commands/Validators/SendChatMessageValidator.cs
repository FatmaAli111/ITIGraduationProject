using FluentValidation;
using ITIGraduationProject.Application.Features.AiChat.Commands.Models;

namespace ITIGraduationProject.Application.Features.AiChat.Commands.Validators
{
    public class SendChatMessageValidator : AbstractValidator<SendChatMessageCommand>
    {
        public SendChatMessageValidator()
        {
            RuleFor(x => x.MessageText)
                .NotEmpty().WithMessage("Message text cannot be empty.")
                .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters.");

            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}