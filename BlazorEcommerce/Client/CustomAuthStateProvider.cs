using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorEcommerce.Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorageService , HttpClient http)
        {
            _localStorageService = localStorageService;
            _http = http;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // ดึง token จาก storage
           string authToken = await _localStorageService.GetItemAsStringAsync("authToken");
            // Identity จะมีเพียงหนึ่งเดียว
            // เก็บค่าต่าง ๆ ของผู้ใช้งาน
            var identity = new ClaimsIdentity();

            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(authToken))
            {
                try
                {
                    identity = new ClaimsIdentity(ParseClaimsFromJwt(authToken), "jwt");
                    _http.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer" , authToken.Replace("\"",""));
                }
                catch 
                {
                    await _localStorageService.RemoveItemAsync("authToken");
                    identity = new ClaimsIdentity();
                }

            }
            // ข้อมูลของผู้ใช้งาน
            var user = new ClaimsPrincipal(identity);
            // นำมาเก็บใน state
            var state = new AuthenticationState(user);

            // เก็บไว้ใน state ทำให้รู้จักทั้งระบบ
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 = base64 + "==";
                    break;
                case 3:
                    base64 = base64 + "=";
                    break;
            }
            return Convert.FromBase64String(base64);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            // ส่วนข้อมูล
            var payload = jwt.Split(".")[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = keyValuePairs.Select(kvp => new Claim(kvp.Key , kvp.Value.ToString()));

            return claims;
        }
    }
}
