using FluentValidation;
using ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Validations
{
    public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
    {
        public ChangeUserRoleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
            RuleFor(x => x.NewRole)
                .NotEmpty().WithMessage("NewRole is required.")
                .Must(role => new[] { "Admin", "User", "Printer" }.Contains(role))
                .WithMessage("Invalid role. Must be Admin, User, or Printer.");
        }
    }
}
