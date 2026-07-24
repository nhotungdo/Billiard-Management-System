using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BilliardManagement.Web.Json;
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

        protected void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private static string FormatErrorMessage(HttpResponseMessage response, string content)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "Phiên đăng nhập đã hết hạn hoặc không có quyền (Unauthorized). Vui lòng đăng nhập lại.";
            }

            var errMsg = $"API Error ({response.StatusCode})";
            try
            {
                var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(content, ApiJson.Options);
                if (apiErr != null && !string.IsNullOrEmpty(apiErr.Message))
                {
                    errMsg = apiErr.Message;
                }
            }
            catch { }
            return errMsg;
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, content));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<TResponse?> PostMultipartAsync<TResponse>(string url, MultipartFormDataContent form)
        {
            AttachToken();
            var response = await _httpClient.PostAsync(url, form);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> PutMultipartAsync(string url, MultipartFormDataContent form)
        {
            AttachToken();
            var response = await _httpClient.PutAsync(url, form);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            return true;
        }

        protected async Task<TResponse?> PutMultipartAsync<TResponse>(string url, MultipartFormDataContent form)
        {
            AttachToken();
            var response = await _httpClient.PutAsync(url, form);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }
        
        protected async Task<TResponse?> PostWithoutBodyAsync<TResponse>(string url)
        {
            AttachToken();
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> PutAsync<TRequest>(string url, TRequest data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            return true;
        }

        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> DeleteAsync(string url)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            return true;
        }

        protected async Task<TResponse?> DeleteAsync<TResponse>(string url)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, ApiJson.Options);
            return apiResponse != null ? apiResponse.Data : default;
        }

        protected async Task<bool> PatchAsync(string url)
        {
            AttachToken();
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception(FormatErrorMessage(response, responseContent));
            }
            return true;
        }
    }
}
