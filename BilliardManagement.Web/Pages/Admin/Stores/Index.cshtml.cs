using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Stores
{
    public class IndexModel : SuperAdminPageModel
    {
        private readonly StoreService _storeService;

        public IndexModel(StoreService storeService)
        {
            _storeService = storeService;
        }

        public List<StoreDto> Stores { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateStoreRequest NewStore { get; set; } = new();

        [BindProperty]
        public UpdateStoreRequest EditStore { get; set; } = new();

        [BindProperty]
        public Guid EditStoreId { get; set; }

        [BindProperty]
        public CreateStoreAdminRequest NewAdmin { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            try
            {
                await _storeService.CreateStoreAsync(NewStore);
                TempData["SuccessMessage"] = "Tạo cửa hàng thành công.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid || EditStoreId == Guid.Empty)
            {
                await LoadAsync();
                return Page();
            }

            try
            {
                await _storeService.UpdateStoreAsync(EditStoreId, EditStore);
                TempData["SuccessMessage"] = "Cập nhật cửa hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
        {
            try
            {
                await _storeService.DeactivateStoreAsync(id);
                TempData["SuccessMessage"] = "Đã vô hiệu hóa cửa hàng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivateAsync(Guid id)
        {
            try
            {
                var store = await _storeService.GetStoreByIdAsync(id);
                if (store == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy cửa hàng.";
                    return RedirectToPage();
                }

                await _storeService.UpdateStoreAsync(id, new UpdateStoreRequest
                {
                    Name = store.Name,
                    Address = store.Address,
                    PhoneNumber = store.PhoneNumber,
                    Email = store.Email,
                    LogoUrl = store.LogoUrl,
                    IsActive = true
                });
                TempData["SuccessMessage"] = "Đã kích hoạt cửa hàng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAdminAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            try
            {
                await _storeService.CreateStoreAdminAsync(NewAdmin);
                TempData["SuccessMessage"] = "Tạo tài khoản Admin cho cửa hàng thành công.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                var result = await _storeService.GetAllStoresAsync();
                Stores = result ?? new List<StoreDto>();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
