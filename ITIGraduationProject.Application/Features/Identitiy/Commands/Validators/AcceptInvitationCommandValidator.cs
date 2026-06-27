using FluentValidation;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Models;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Validators
{
    public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
    {
        public AcceptInvitationCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
