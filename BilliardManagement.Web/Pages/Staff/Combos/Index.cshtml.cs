using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Combos
{
    public class IndexModel : AdminOrStaffPageModel
    {
        private readonly ComboService _comboService;

        public IndexModel(ComboService comboService)
        {
            _comboService = comboService;
        }

        public List<ComboDto> ActiveCombos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var combosResult = await _comboService.GetAllCombosAsync(Search, activeOnly: true);
                ActiveCombos = combosResult ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return Page();
        }
    }
}
