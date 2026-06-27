using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using Mapster;

namespace ITIGraduationProject.Application.Common.Mappings
{
    public class CartMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CartItem, CartItemDto>()
                .Map(dest => dest.CartItemId,              src => src.Id)
                .Map(dest => dest.ProductId,               src => src.ProductId)
                .Map(dest => dest.ProductName,             src => src.Product != null ? src.Product.Name : string.Empty)
                .Map(dest => dest.ProductImage,            src => src.Product != null ? src.Product.PreviewImageURL : string.Empty)
                .Map(dest => dest.Quantity,                src => src.Quantity)
                .Map(dest => dest.UnitPrice,               src => src.UnitPrice)
                .Map(dest => dest.TotalPrice,              src => src.Quantity * src.UnitPrice)
                .Map(dest => dest.DesignId,                src => src.DesignId)
                .Map(dest => dest.DesignSnapshotImageUrl,  src => src.Design != null ? src.Design.SnapshotImageURL : null);

            config.NewConfig<Cart, CartDto>()
                .Map(dest => dest.Id,        src => src.Id)
                .Map(dest => dest.UserId,    src => src.UserId)
                .Map(dest => dest.Items,     src => src.CartItems.Adapt<List<CartItemDto>>())
                .Map(dest => dest.TotalCost, src => src.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice))
                .Map(dest => dest.ItemCount, src => src.CartItems.Sum(ci => ci.Quantity));
        }
    }
}
