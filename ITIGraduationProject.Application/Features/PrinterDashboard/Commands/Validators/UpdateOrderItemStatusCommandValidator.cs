using FluentValidation;
using ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Validators
{
    public class UpdateOrderItemStatusCommandValidator : AbstractValidator<UpdateOrderItemStatusCommand>
    {
        public UpdateOrderItemStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Invalid status value.");
        }
    }
}
