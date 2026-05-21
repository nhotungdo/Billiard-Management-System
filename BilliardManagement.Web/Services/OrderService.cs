using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

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
            return await GetAsync<List<OrderDto>>("orders");
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            return await GetAsync<OrderDto>($"orders/{id}");
        }

        public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto request)
        {
            return await PostAsync<CreateOrderDto, OrderDto>("orders", request);
        }
    }
}
