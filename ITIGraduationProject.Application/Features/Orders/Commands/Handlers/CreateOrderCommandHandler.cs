using MediatR;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Handlers
{
    public class CreateOrderCommandHandler : ResponseHandler, IRequestHandler<CreateOrderCommand, Response<string>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;
        public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken) {

            decimal subTotal = 0;
            var orderItemsList = new List<OrderItem>();

            #region Calculate Order Items and Check If Design Exists
            foreach (var item in request.OrderItems) { 

                var design = await _unitOfWork.Designs.GetByIdAsync(item.DesignId);
                if (design == null)
                {
                    return BadRequest<string>($"Design with ID {item.DesignId} does not exist.");
                }

                #region Calculate Price
                var price = design.CalculatedPrice;
                subTotal += price * item.Quantity;
                #endregion

                orderItemsList.Add(new OrderItem
                {
                    DesignId = item.DesignId,
                    Quantity = item.Quantity,
                    UnitPrice = price,
                    PrinterProfileId = item.PrinterProfileId,
                    Status = OrderItemStatus.Pending, 
                    SnapshotImageURL = design.SnapshotImageURL, 
                    PriceBreakdown = $"Size: {design.SelectedSize}, Fabric: {design.SelectedFabric}, Color: {design.SelectedColor}" 
                });
            }
            #endregion

            #region Calculate Discount Amount and Coupon
            decimal discountAmount = 0;
            Guid? couponId = null;

            if (!string.IsNullOrEmpty(request.CouponCode)) { 

                var coupon = await _unitOfWork.Coupons.GetTableNoTracking()
                    .FirstOrDefaultAsync(coupon => coupon.Code == request.CouponCode && coupon.IsActive , cancellationToken);

                if (coupon != null && coupon.ExpiryDate > DateTime.UtcNow && coupon.UsedCount < coupon.UsageLimit 
                    && subTotal >= coupon.MinOrderAmount)
                {
                    couponId = coupon.Id;
                    if (coupon.DiscountType == CouponType.Percentage)
                    {
                        discountAmount = subTotal * (coupon.DiscountValue / 100);
                    }
                    else if (coupon.DiscountType == CouponType.FixedAmount)
                    {
                        discountAmount = coupon.DiscountValue;
                    }
                    else if (coupon.DiscountType == CouponType.FreeShipping)
                    {
                        discountAmount = 0;
                    }
                }
            }
            #endregion

            #region Calculate Tax & Total 
            decimal tax = subTotal * 0.08m; 
            decimal totalAmount = (subTotal + tax) - discountAmount;
            #endregion

            // create order number
            string orderNumber = $"WLY-{DateTime.UtcNow.Year}-{new Random().Next(10000, 99999)}";

            #region the final order will send to Database
            var order = new Order
            {
                UserId = Guid.Parse(request.UserId),
                OrderNumber = orderNumber,
                ReceiverName = request.ReceiverName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                City = request.City,
                DeliveryNotes = request.DeliveryNotes,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                CouponId = couponId,
                OrderStatus = OrderStatus.Pending, 
                PaymentStatus = PaymentStatus.Pending,
                OrderItems = orderItemsList
            };
            #endregion

            #region Save Order to Database
            await _unitOfWork.Orders.AddAsync(order);
            var result = await _unitOfWork.SaveChangesAsync();
            #endregion

            if (result > 0)
            {
                return Success(orderNumber, "Order created successfully.");
            }
            else
            {
                return BadRequest<string>("Failed to place the order. Please try again.");
            }
        }
        #endregion

    }
}
