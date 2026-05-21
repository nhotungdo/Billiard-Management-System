using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Staff
{
    public class IndexModel : PageModel
    {
        private readonly StaffService _staffService;

        public IndexModel(StaffService staffService)
        {
            _staffService = staffService;
        }

        public List<StaffDto> StaffList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var list = await _staffService.GetAllStaffAsync();
                if (list != null)
                {
                    StaffList = list;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load staff: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync(Guid id, int role)
        {
            try
            {
                var staff = await _staffService.UpdateStaffRoleAsync(id, role);
                if (staff != null)
                {
                    TempData["SuccessMessage"] = $"Role updated successfully for {staff.FullName}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update staff role.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating staff role: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
