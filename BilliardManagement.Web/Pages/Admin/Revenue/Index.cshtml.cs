using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BilliardManagement.Web.Pages.Admin.Revenue
{
    public class IndexModel : AdminPageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly StaffService _staffService;

        public IndexModel(IHttpClientFactory clientFactory, StaffService staffService)
        {
            _clientFactory = clientFactory;
            _staffService = staffService;
        }

        public RevenueSummaryDto SummaryData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? StaffId { get; set; }

        public List<SelectListItem> StaffList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            var client = _clientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Fetch Staff list for filter
            try
            {
                var staffs = await _staffService.GetAllStaffAsync();
                if (staffs != null)
                {
                    // Only roles: Staff
                    StaffList = staffs.Where(u => u.Role == 2).Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FullName
                    }).ToList();
                }
            }
            catch { }

            var queryParams = new List<string>();
            if (FromDate.HasValue) queryParams.Add($"fromDate={FromDate.Value.ToString("o")}");
            if (ToDate.HasValue) queryParams.Add($"toDate={ToDate.Value.ToString("o")}");
            if (StaffId.HasValue) queryParams.Add($"staffId={StaffId.Value}");
            
            var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            var response = await client.GetAsync($"/api/revenues/total{query}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                SummaryData = JsonSerializer.Deserialize<RevenueSummaryDto>(content, options) ?? new();
            }

            return Page();
        }
    }
}
