using BlazorEcommerce.Shared;

namespace BlazorEcommerce.Server.Services.CartService
{
    public interface ICartService
    {
        Task<ServiceResponse<List<CartProductResponse>>> GetCartProducts(List<CartItem> cartItems);
        // ทำหน้าที่เก็บ สินค้า ในตะกร้าสินค้าของ local ของ Client
        Task<ServiceResponse<List<CartProductResponse>>> StoreCartItems (List<CartItem> cartItems);
        Task<ServiceResponse<int>> GetCartItemCount();
        Task<ServiceResponse<List<CartProductResponse>>> GetDbCartProducts(int? userId = null);
        Task<ServiceResponse<bool>> AddToCart(CartItem cartItem);
        Task<ServiceResponse<bool>> UpdateQuantity(CartItem cartItem);
        Task<ServiceResponse<bool>> RemoveItemFromCart(int productId , int productTypeId);
    }
}
