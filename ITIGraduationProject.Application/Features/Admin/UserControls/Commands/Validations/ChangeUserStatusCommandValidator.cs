using FluentValidation;
using ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Validations
{
    public class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
    {
        public ChangeUserStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
