using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BilliardManagement.Web.Pages.Admin.Revenue
{
    public class StaffDetailsModel : AdminPageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public StaffDetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public PersonalRevenueDto StaffRevenueData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid StaffId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            if (StaffId == Guid.Empty) return RedirectToPage("/Admin/Revenue/Index");

            var client = _clientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var queryParams = new List<string>();
            if (FromDate.HasValue) queryParams.Add($"fromDate={FromDate.Value.ToString("o")}");
            if (ToDate.HasValue) queryParams.Add($"toDate={ToDate.Value.ToString("o")}");
            
            var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            var response = await client.GetAsync($"/api/revenues/staff/{StaffId}{query}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                StaffRevenueData = JsonSerializer.Deserialize<PersonalRevenueDto>(content, options) ?? new();
            }
            else
            {
                return RedirectToPage("/Admin/Revenue/Index");
            }

            return Page();
        }
    }
}
