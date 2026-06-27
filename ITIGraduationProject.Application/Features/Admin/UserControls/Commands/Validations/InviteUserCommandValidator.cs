using FluentValidation;
using ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Validations
{
    public class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
    {
        public InviteUserCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(r => r == Roles.Admin || r == Roles.Printer)
                .WithMessage("Role must be either Admin or Printer.");

            RuleFor(x => x.SupportedFabrics)
                .NotNull()
                .When(x => x.Role == Roles.Printer)
                .WithMessage("Supported fabrics is required for printers.");

            RuleFor(x => x.SupportedFabrics)
                .Must(IsValidFabricSelection)
                .When(x => x.Role == Roles.Printer && x.SupportedFabrics.HasValue)
                .WithMessage("Select at least one valid supported fabric.");

            RuleFor(x => x.SupportedPrintMethods)
                .NotNull()
                .When(x => x.Role == Roles.Printer)
                .WithMessage("Supported print methods is required for printers.");

            RuleFor(x => x.SupportedPrintMethods)
                .Must(IsValidPrintMethodSelection)
                .When(x => x.Role == Roles.Printer && x.SupportedPrintMethods.HasValue)
                .WithMessage("Select at least one valid supported print method.");
        }

        private static bool IsValidFabricSelection(FabricType? value)
        {
            const FabricType allowed = FabricType.Cotton | FabricType.Polyester |
                                       FabricType.Wool | FabricType.Silk | FabricType.Linen;
            return value.HasValue && value.Value != 0 && (value.Value & ~allowed) == 0;
        }

        private static bool IsValidPrintMethodSelection(PrintMethodType? value)
        {
            const PrintMethodType allowed = PrintMethodType.DirectToGarment |
                                            PrintMethodType.ScreenPrinting |
                                            PrintMethodType.HeatTransfer |
                                            PrintMethodType.Sublimation |
                                            PrintMethodType.Embroidery;
            return value.HasValue && value.Value != 0 && (value.Value & ~allowed) == 0;
        }
    }
}
