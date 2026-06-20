using FluentValidation;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Validators
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator() { 

            RuleFor(x => x.UserId).NotEmpty().WithMessage("User identity is required.");

            RuleFor(x => x.ReceiverName).NotEmpty().WithMessage("Receiver name is required.");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^01[0125]\d{8}$").WithMessage("Invalid Egyptian phone number."); 

            RuleFor(x => x.Address).NotEmpty().WithMessage("Delivery address is required.");

            RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.OrderItems).NotEmpty().WithMessage("Order must contain at least one item.");
        }
    }
}
