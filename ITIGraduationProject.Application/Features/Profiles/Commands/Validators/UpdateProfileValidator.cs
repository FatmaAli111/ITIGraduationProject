using FluentValidation;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;

namespace ITIGraduationProject.Application.Features.Profiles.Commands.Validators
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator () {

            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required.")
                .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");


        }
    }
}
