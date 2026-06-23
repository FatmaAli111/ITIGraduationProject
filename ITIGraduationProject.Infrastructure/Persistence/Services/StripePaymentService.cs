using Stripe;
using Stripe.Checkout;
using ITIGraduationProject.Application.Interfaces.IServices;
using ITIGraduationProject.Domain.Entities.ECommerce;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ITIGraduationProject.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        #region Configuration Injection
        private readonly IConfiguration _configuration;
        public StripePaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion
        public async Task<string> CreatePaymentSessionAsync(Order order)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(order.TotalAmount * 100), 
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order {order.OrderNumber}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/payment/success",
                CancelUrl = "http://localhost:4200/payment/cancel"
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}