using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BilliardManagement.Web.Pages.Staff.Revenue
{
    public class IndexModel : StaffPageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public IndexModel(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        public PersonalRevenueDto RevenueData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            var client = _clientFactory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var query = "";
            if (FilterDate.HasValue)
            {
                var startOfDay = FilterDate.Value.Date.ToString("o");
                var endOfDay = FilterDate.Value.Date.AddDays(1).AddTicks(-1).ToString("o");
                query = $"?fromDate={startOfDay}&toDate={endOfDay}";
            }

            var response = await client.GetAsync($"/api/revenues/personal{query}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                RevenueData = JsonSerializer.Deserialize<PersonalRevenueDto>(content, options) ?? new();
            }

            return Page();
        }
    }
}
