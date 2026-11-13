namespace BilliardManagement.Business.DTOs;

public class StoreReportDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = "";
    public bool IsActive { get; set; }
    public int TableCount { get; set; }
    public int ActiveTables { get; set; }
    public int StaffCount { get; set; }
    public int Sessions { get; set; }
    public int SessionsToday { get; set; }
    public decimal Revenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal? GrowthPercent { get; set; }
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
}

public class SystemOverviewDto
{
    public int TotalStores { get; set; }
    public int ActiveTables { get; set; }
    public int ActiveUsers { get; set; }
    public decimal TodayRevenue { get; set; }
    public List<StoreReportDto> Stores { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
}
