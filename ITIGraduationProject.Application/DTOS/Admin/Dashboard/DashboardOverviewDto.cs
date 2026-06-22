namespace ITIGraduationProject.Application.DTOS.Admin.Dashboard
{
    public class DashboardOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalTemplates { get; set; }
        public int PublicTemplates { get; set; }
        public int PendingModerationReports { get; set; }
    }
}
