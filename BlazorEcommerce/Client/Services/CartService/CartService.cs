using Blazored.LocalStorage;

namespace BlazorEcommerce.Client.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        private readonly IAuthService _authService;

        public CartService(ILocalStorageService localStorage , HttpClient http,
            IAuthService authService)
        {
            _localStorage = localStorage;
            _http = http;
            _authService = authService;
        }

        public event Action OnChange;

        public async Task AddToCart(CartItem cartItem)
        {
            if (await _authService.IsUserAuthenticated())
            {
                await _http.PostAsJsonAsync("api/cart/add", cartItem);
            }
            else //กรณีที่ยังไม่ login จะเก็บไว้ใน local ก่อน
            {
                var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
                if (cart == null)
                {
                    cart = new List<CartItem>(); // สร้างตัวตนให้มัน
                }
                var sameItem = cart.Find(x => x.ProductId == cartItem.ProductId &&
                    x.ProductTypeId == cartItem.ProductTypeId);
                if (sameItem == null)
                {
                    cart.Add(cartItem); // มีตัวตนแล้วเลย ADD ได้
                }
                else
                {
                    sameItem.Quantity += cartItem.Quantity;
                }
                await _localStorage.SetItemAsync("cart", cart);
            }
            await GetCartItemCount(); 
        }

        public async Task GetCartItemCount()
        {
            if (await _authService.IsUserAuthenticated())
            {
                var result = await _http.GetFromJsonAsync<ServiceResponse<int>>("api/cart/count");
                var count = result.Data;
                await _localStorage.SetItemAsync<int>("cartItemsCount", count);
            }
            else
            {
                var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
                await _localStorage.SetItemAsync<int>("cartItemsCount", cart != null ? cart.Count : 0 );
            }
            // ให้เกิดการเปลี่ยนแปลง
            OnChange.Invoke();
        }

        //public async Task<List<CartItem>> GetCartItems()
        //{
        //    await GetCartItemCount();
        //    var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
        //    if (cart == null)
        //    {
        //        cart = new List<CartItem>(); // สร้างตัวตนให้มัน
        //    }
        //    return cart;
        //}

        public async Task<List<CartProductResponse>> GetCartProducts()
        {
            if (await _authService.IsUserAuthenticated())
            {
                var response = await _http.GetFromJsonAsync<ServiceResponse<List<CartProductResponse>>>("api/cart");
                return response.Data;
            }
            else
            {
                var cartItems = await _localStorage.GetItemAsync<List<CartItem>>("cart");
                if (cartItems == null)
                    return new List<CartProductResponse>();
                var response = await _http.PostAsJsonAsync("api/cart/products" , cartItems);
                var cartProducts = await response.Content.ReadFromJsonAsync<ServiceResponse<List<CartProductResponse>>>();
                return cartProducts.Data;
            }
        }
        public async Task RemoveProductFromCart(int productId, int productTypeId)
        {
            if (await _authService.IsUserAuthenticated())
            {
               await _http.DeleteAsync($"api/cart/{productId}/{productTypeId}");
            }
            else
            {
                var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
                if (cart == null) return;

                var cartItem = cart.Find(e => e.ProductId == productId && e.ProductTypeId == productTypeId);

                if (cart is not null)
                {
                    cart.Remove(cartItem);
                    await _localStorage.SetItemAsync("cart", cart);
                    //await GetCartItemCount();
                }
            }
        }

        public async Task StoreCartItems(bool emptyLocalCart = false)
        {
            var localCart = await _localStorage.GetItemAsync<List<CartItem>>("cart");

            if (localCart == null) return;

            await _http.PostAsJsonAsync("api/cart" , localCart);

            //ถ้าเป็น true ให้ลบสินค้าในตะกร้าของ local
            if (emptyLocalCart)
            {
                await _localStorage.RemoveItemAsync("cart");
            }
        }

        public async Task UpdateQuantity(CartProductResponse product)
        {
            if (await _authService.IsUserAuthenticated())
            {
                var request = new CartItem
                {
                    ProductId = product.ProductId,
                    Quantity= product.Quantity,
                    ProductTypeId = product.ProductTypeId
                };
                await _http.PutAsJsonAsync("api/cart/update-quantity" , request);
            }

            var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
            if (cart == null)
            {
                return;
            }
            var cartItem = cart.Find(e => e.ProductId == product.ProductId 
                && e.ProductTypeId == product.ProductTypeId);

            if (cart is not null)
            {
                cartItem.Quantity = product.Quantity;
                await _localStorage.SetItemAsync("cart", cart);
            }
        }
       
        
    }
}
