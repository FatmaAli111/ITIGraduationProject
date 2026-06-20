using ITIGraduationProject.Application.DTOS.Orders;
using ITIGraduationProject.Domain.Entities.ECommerce;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Mapping
{
    public class OrderMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            #region Create the Configuration
            config.NewConfig<Order, OrderListDTO>()
                .Map(dest => dest.OrderId, src => src.Id)
                .Map(dest => dest.PlacedDate, src => src.CreatedAt)                                                    
                .Map(dest => dest.TrackingNumber, src => src.Shipment != null ? src.Shipment.TrackingNumber : null)  // set static value for shipment to don't cause any NullReferenceException
                .Map(dest => dest.EstimatedDeliveryDate, src => src.Shipment != null ? src.Shipment.EstimatedDeliveryDate : System.DateTime.UtcNow.AddDays(5))
                .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
                .Map(dest => dest.Subtotal, src => src.SubTotal)
                .Map(dest => dest.Total, src => src.TotalAmount)
                .Map(dest => dest.Tax, src => src.SubTotal * 0.08m) 
                .Map(dest => dest.OrderItems, src => src.OrderItems);

            // Mapping for each order Item
            config.NewConfig<OrderItem, OrderDetailItemDTO>()
                .Map(dest => dest.DesignId, src => src.DesignId)
                // Get Design Name from Navigation Property 
                .Map(dest => dest.DesignName, src => src.Design != null && src.Design.Product != null ? src.Design.Product.Name : "Custom Design")
                .Map(dest => dest.VariationDetails, src => src.PriceBreakdown)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.UnitPrice, src => src.UnitPrice)
                .Map(dest => dest.SnapshotImageURL, src => src.SnapshotImageURL);
            #endregion
        }
    }
}
