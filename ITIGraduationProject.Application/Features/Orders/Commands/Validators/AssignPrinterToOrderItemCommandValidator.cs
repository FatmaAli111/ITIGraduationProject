using FluentValidation;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Validators
{
    public class AssignPrinterToOrderItemCommandValidator : AbstractValidator<AssignPrinterToOrderItemCommand>
    {
        public AssignPrinterToOrderItemCommandValidator()
        {
            RuleFor(x => x.OrderItemId)
                .NotEmpty().WithMessage("OrderItemId is required.");
            RuleFor(x => x.PrinterProfileId)
                .NotEmpty().WithMessage("PrinterProfileId is required.");
        }
    }
}
