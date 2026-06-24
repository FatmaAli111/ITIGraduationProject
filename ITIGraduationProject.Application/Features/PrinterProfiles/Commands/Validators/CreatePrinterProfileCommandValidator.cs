using FluentValidation;
using ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Validators
{
    public class CreatePrinterProfileCommandValidator : AbstractValidator<CreatePrinterProfileCommand>
    {
        public CreatePrinterProfileCommandValidator()
        {
            RuleFor(x => x.SupportedFabrics)
                .NotEmpty().WithMessage("SupportedFabrics is required.");
            RuleFor(x => x.SupportedPrintMethods)
                .NotEmpty().WithMessage("SupportedPrintMethods is required.");
        }
    }
}
