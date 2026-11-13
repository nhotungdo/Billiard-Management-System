using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Reports;

public class CompareModel(StoreService stores) : SuperAdminPageModel
{
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public Guid[] StoreIds { get; set; } = Array.Empty<Guid>();
    public StoreComparisonViewModel Report { get; set; } = new() { Title = "So sánh cửa hàng" };

    public async Task OnGetAsync()
    {
        Report.FromDate = FromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-6);
        Report.ToDate = ToDate?.Date ?? DateTime.UtcNow.Date;
        Report.StoreIds = StoreIds;
        try
        {
            Report.Stores = await stores.GetAllStoresAsync() ?? new();
            if (!ModelState.IsValid) throw new Exception("Bộ lọc không hợp lệ.");
            Report.Rows = await stores.CompareAsync(StoreIds, Report.FromDate, Report.ToDate) ?? new();
        }
        catch (Exception ex) { Report.ErrorMessage = ex.Message; }
    }
}
