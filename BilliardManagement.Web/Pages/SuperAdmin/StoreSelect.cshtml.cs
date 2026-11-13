using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.SuperAdmin;

public class StoreSelectModel(StoreService stores) : SuperAdminPageModel
{
    public List<StoreDto> Stores { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public async Task OnGetAsync()
    {
        try { Stores = await stores.GetAllStoresAsync() ?? new(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }
    public async Task<IActionResult> OnPostSelectAsync(Guid storeId)
    {
        try
        {
            var store = await stores.GetStoreByIdAsync(storeId);
            if (store == null || !store.IsActive) throw new Exception("Cửa hàng không tồn tại hoặc đã ngừng hoạt động.");
            HttpContext.Session.SetString("ViewAsStoreId", store.Id.ToString());
            HttpContext.Session.SetString("ViewAsStoreName", store.Name);
            HttpContext.Session.SetString("ViewAsStoreLogo", store.LogoUrl ?? "");
            return RedirectToPage("/Admin/Tables/Index");
        }
        catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToPage(); }
    }
    public IActionResult OnPostClear()
    {
        HttpContext.Session.Remove("ViewAsStoreId");
        HttpContext.Session.Remove("ViewAsStoreName");
        HttpContext.Session.Remove("ViewAsStoreLogo");
        return RedirectToPage("/Admin/Dashboard/Index");
    }
}
