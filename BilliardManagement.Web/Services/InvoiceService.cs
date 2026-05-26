using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class InvoiceService : BaseApiService
    {
        public InvoiceService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<InvoiceDto>?> GetAllInvoicesAsync()
        {
            var result = await GetAsync<PagedResult<InvoiceDto>>("bills?pageSize=1000");
            return result?.Items;
        }

        public async Task<PagedResult<InvoiceDto>?> GetPagedInvoicesAsync(int pageNumber, int pageSize, bool? isPaid = null, int? paymentMethod = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"bills?pageNumber={pageNumber}&pageSize={pageSize}";
            if (isPaid.HasValue) url += $"&isPaid={isPaid.Value}";
            if (paymentMethod.HasValue) url += $"&paymentMethod={paymentMethod.Value}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<InvoiceDto>>(url);
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id)
        {
            return await GetAsync<InvoiceDto>($"bills/{id}");
        }

        public async Task<InvoiceDto?> GenerateInvoiceAsync(Guid sessionId, CreateInvoiceDto request)
        {
            return await PostAsync<CreateInvoiceDto, InvoiceDto>($"bills/generate/{sessionId}", request);
        }

        public async Task<InvoiceDto?> PayInvoiceAsync(Guid id)
        {
            return await PostWithoutBodyAsync<InvoiceDto>($"bills/pay/{id}");
        }
    }
}
