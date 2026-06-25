using FluentValidation;
using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Validators
{
    public class GetMyAssignedOrderItemsQueryValidator : AbstractValidator<GetMyAssignedOrderItemsQuery>
    {
        public GetMyAssignedOrderItemsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.");
        }
    }
}
