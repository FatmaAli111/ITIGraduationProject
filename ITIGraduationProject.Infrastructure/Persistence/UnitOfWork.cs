using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Infrastructure.Persistence.Repositories;
using System.Threading.Tasks;

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
        public IProductImageRepository ProductImages { get; }
        public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);
        private ICategoryRepository? _categories;

        public ICommunityInteractionRepository CommunityInteractions =>
        _communityInteractions ??= new CommunityInteractionRepository(_context);
        private ICommunityInteractionRepository? _communityInteractions;

        public IPrinterProfileRepository PrinterProfiles =>
        _printerProfiles ??= new PrinterProfileRepository(_context);
        private IPrinterProfileRepository? _printerProfiles;

        public IOrderItemRepository OrderItems =>
        _orderItems ??= new OrderItemRepository(_context);
        private IOrderItemRepository? _orderItems;

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
            IRefreshTokenRepository refreshTokens,
            IProductImageRepository productImages)
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
            ProductImages = productImages;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
