using FluentValidation;
using ITIGraduationProject.Application.Features.Community.Commands.Models;

namespace ITIGraduationProject.Application.Features.Community.Commands.Validators
{
    public class ReportTemplateCommandValidator : AbstractValidator<ReportTemplateCommand>
    {
        public ReportTemplateCommandValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Report reason is required.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
        }
    }
}
