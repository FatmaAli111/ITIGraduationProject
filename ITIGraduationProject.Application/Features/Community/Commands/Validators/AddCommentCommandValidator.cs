using FluentValidation;
using ITIGraduationProject.Application.Features.Community.Commands.Models;

namespace ITIGraduationProject.Application.Features.Community.Commands.Validators
{
    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required.")
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
        }
    }
}
