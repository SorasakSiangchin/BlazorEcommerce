namespace BlazorEcommerce.Server.Services.ProductService
{
    public interface IProductServices
    {
        Task<ServiceResponse<List<Product>>> GetProductsAsync();
        Task<ServiceResponse<Product>> GetProductAsync(int productId);
    }
}
