namespace BilliardManagement.Web.Models;

public class StoreComparisonViewModel
{
    public string Title { get; set; } = "Báo cáo doanh thu hệ thống";
    public DateTime FromDate { get; set; } = DateTime.UtcNow.Date.AddDays(-6);
    public DateTime ToDate { get; set; } = DateTime.UtcNow.Date;
    public Guid[] StoreIds { get; set; } = Array.Empty<Guid>();
    public List<StoreDto> Stores { get; set; } = new();
    public List<StoreReportDto> Rows { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
