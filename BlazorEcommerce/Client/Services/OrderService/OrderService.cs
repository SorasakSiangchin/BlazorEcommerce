using Microsoft.AspNetCore.Components;

namespace BlazorEcommerce.Client.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _navigationManager;
        private readonly IAuthService _authService;

        public OrderService(HttpClient http , 
            NavigationManager navigationManager ,
            IAuthService authService)
        {
            _http = http;
            _navigationManager = navigationManager;
            _authService = authService;
        }

        public async Task<OrderDetailsResponse> GetOrderDetails(int orderId)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<OrderDetailsResponse>>($"api/Order/{orderId}");
            return result.Data;
        }

        public async Task<List<OrderOverviewResponse>> GetOrders()
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<OrderOverviewResponse>>>("api/Order");
            return result.Data;
        }

        public async Task<string> PlaceOrder()
        {
            if (await _authService.IsUserAuthenticated())
            {
                var result = await _http.PostAsync("api/payment/checkout", null);
                var url = await result.Content.ReadAsStringAsync();
                return url;
            }
            else
            {
                return "login";
            }
        }
    }
}
