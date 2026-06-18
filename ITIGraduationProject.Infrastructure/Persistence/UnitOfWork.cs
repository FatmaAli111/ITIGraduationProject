using System.Threading.Tasks;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;

namespace ITIGraduationProject.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IProductRepository Products { get; }
        public IDesignRepository Designs { get; }
        public IOrderRepository Orders { get; }
        public IUserRepository Users { get; }
        public ITemplateRepository Templates { get; }
        public ICouponRepository Coupons { get; }
        public IModerationReportRepository ModerationReports { get; }
        public IAiChatSessionRepository AiChatSessions { get; }
        public IShipmentRepository Shipments { get; }
        public IRewardRepository Rewards { get; }
        public IGraphicAssetRepository GraphicAssets { get; }
        public INotificationRepository Notifications { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        public UnitOfWork(
            AppDbContext context,
            IProductRepository products,
            IDesignRepository designs,
            IOrderRepository orders,
            IUserRepository users,
            ITemplateRepository templates,
            ICouponRepository coupons,
            IModerationReportRepository moderationReports,
            IAiChatSessionRepository aiChatSessions,
            IShipmentRepository shipments,
            IRewardRepository rewards,
            IGraphicAssetRepository graphicAssets,
            INotificationRepository notifications,
            IRefreshTokenRepository refreshTokens)
        {
            _context = context;

            Products = products;
            Designs = designs;
            Orders = orders;
            Users = users;
            Templates = templates;
            Coupons = coupons;
            ModerationReports = moderationReports;
            AiChatSessions = aiChatSessions;
            Shipments = shipments;
            Rewards = rewards;
            GraphicAssets = graphicAssets;
            Notifications = notifications;
            RefreshTokens = refreshTokens;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
