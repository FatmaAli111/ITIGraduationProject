using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;


namespace ITIGraduationProject.Application.Features.Orders.Commands.Handlers
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);

            if (order == null)
                return false;

            order.OrderStatus = request.NewStatus;
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0;
        }
    }
}