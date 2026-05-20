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
            return await GetAsync<List<InvoiceDto>>("bills");
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
