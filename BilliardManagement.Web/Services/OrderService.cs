using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Services
{
    public class OrderService : BaseApiService
    {
        public OrderService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<OrderDto>?> GetAllOrdersAsync()
        {
            var result = await GetAsync<PagedResult<OrderDto>>("orders?pageSize=1000");
            return result?.Items;
        }

        public async Task<PagedResult<OrderDto>?> GetPagedOrdersAsync(int pageNumber, int pageSize, int? status = null, Guid? sessionId = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"orders?pageNumber={pageNumber}&pageSize={pageSize}";
            if (status.HasValue) url += $"&status={status.Value}";
            if (sessionId.HasValue) url += $"&sessionId={sessionId.Value}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<OrderDto>>(url);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            return await GetAsync<OrderDto>($"orders/{id}");
        }

        public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto request)
        {
            return await PostAsync<CreateOrderDto, OrderDto>("orders", request);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, int status)
        {
            return await PutAsync($"orders/{id}/status", new { status });
        }
    }
}
