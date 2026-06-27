using System.Threading.Tasks;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Interfaces.IRepositories;

namespace ITIGraduationProject.Application.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IDesignRepository Designs { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }
        ITemplateRepository Templates { get; }
        ICouponRepository Coupons { get; }
        IModerationReportRepository ModerationReports { get; }
        IAiChatSessionRepository AiChatSessions { get; }
        IShipmentRepository Shipments { get; }
        IRewardRepository Rewards { get; }
        IGraphicAssetRepository GraphicAssets { get; }
        INotificationRepository Notifications { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        ICategoryRepository Categories { get; }
        ICommunityInteractionRepository CommunityInteractions { get; }
        IProductImageRepository ProductImages { get; }
        IPrinterProfileRepository PrinterProfiles { get; }
        IAiChatMessageRepository AiChatMessages { get; }
        IOrderItemRepository OrderItems { get; }
        ICartRepository Carts => throw new System.NotImplementedException();
        ICartItemRepository CartItems => throw new System.NotImplementedException();

        Task<int> SaveChangesAsync();
    }
}



