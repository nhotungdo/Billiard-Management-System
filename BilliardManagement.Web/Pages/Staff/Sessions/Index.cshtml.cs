using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Sessions
{
    public class IndexModel : StaffPageModel
    {
        private readonly SessionService _sessionService;
        public IndexModel(SessionService sessionService) { _sessionService = sessionService; }

        public List<SessionDto> ActiveSessions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _sessionService.GetActiveSessionsAsync();
                ActiveSessions = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
