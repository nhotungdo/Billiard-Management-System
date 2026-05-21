using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Staff
{
    public class IndexModel : AdminPageModel
    {
        private readonly StaffService _staffService;
        public IndexModel(StaffService staffService) { _staffService = staffService; }

        public List<StaffDto> StaffList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _staffService.GetAllStaffAsync();
                StaffList = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
