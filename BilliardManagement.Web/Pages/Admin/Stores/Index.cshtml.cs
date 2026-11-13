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
        public List<StoreReportDto> Statistics { get; set; } = new();
        public List<UserDto> Admins { get; set; } = new();
        [BindProperty(SupportsGet = true)] public Guid? DetailId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ActiveModal { get; set; }

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
            ModelState.Clear();
            TryValidateModel(NewStore, nameof(NewStore));
            if (!ModelState.IsValid)
            {
                ActiveModal = "createStoreModal";
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
            ModelState.Clear();
            TryValidateModel(EditStore, nameof(EditStore));
            if (!ModelState.IsValid || EditStoreId == Guid.Empty)
            {
                ActiveModal = "editStoreModal";
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
            ModelState.Clear();
            TryValidateModel(NewAdmin, nameof(NewAdmin));
            if (NewAdmin.StoreId == Guid.Empty)
                ModelState.AddModelError("NewAdmin.StoreId", "Vui lòng chọn cửa hàng.");
            if (!ModelState.IsValid)
            {
                ActiveModal = "createAdminModal";
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
                var today = DateTime.UtcNow.Date;
                Statistics = await _storeService.CompareAsync(null, new DateTime(today.Year, today.Month, 1), today) ?? new();
                Admins = (await _storeService.GetUsersAsync() ?? new()).Where(u => u.Role == 1).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
