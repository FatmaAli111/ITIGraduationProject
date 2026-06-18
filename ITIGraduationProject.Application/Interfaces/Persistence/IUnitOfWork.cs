using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;

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

        Task<int> SaveChangesAsync();
    }
}
