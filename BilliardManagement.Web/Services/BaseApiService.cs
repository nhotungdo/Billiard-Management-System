using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BaseApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _httpContextAccessor = httpContextAccessor;
            AttachToken();
        }

        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse != null ? apiResponse.Data : default;
        }
        
        protected async Task<TResponse?> PostWithoutBodyAsync<TResponse>(string url)
        {
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> PutAsync<TRequest>(string url, TRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            return true;
        }

        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errMsg = $"API Error ({response.StatusCode})";
                try
                {
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                    {
                        errMsg = apiErr.Message;
                    }
                }
                catch {}
                throw new Exception(errMsg);
            }
            return true;
        }
    }
}
