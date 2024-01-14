
using BlazorEcommerce.Server.Services.ProductService;
using BlazorEcommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorEcommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productServices;

        public ProductController(IProductService productServices)
        {
            _productServices = productServices;
        }

        [HttpGet("admin") , Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<List<Product>>>> GetAdminProducts()
        {
            var result = await _productServices.GetAdminProducts();
            return Ok(result);
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<Product>>> CreateProduct(Product product)
        {
            var result = await _productServices.CreateProduct(product);
            return Ok(result);
        }

        [HttpPut, Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<List<Product>>>> UpdateProduct(Product product)
        {
            var result = await _productServices.UpdateProduct(product);
            return Ok(result);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<List<Product>>>> DeleteProduct(int id)
        {
            var result = await _productServices.DeleteProduct(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Product>>>> GetProducts()
        {
          var result = await _productServices.GetProductsAsync();
          return Ok(result);
        }

        [HttpGet]
        [Route("{productId:int}")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetProduct(int productId)
        {
            var result = await _productServices.GetProductAsync(productId);
            return Ok(result);
        }

        [HttpGet]
        [Route("category/{categoryUrl}")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetProductsByCategory(string categoryUrl)
        {
            var result = await _productServices.GetProductsByCategory(categoryUrl);
            return Ok(result);
        }

        [HttpGet]
        [Route("search/{searchText}/{page}")]
        public async Task<ActionResult<ServiceResponse<ProductSearchResult>>> SearchProducts (string searchText , int page = 1)
        {
            var result = await _productServices.SearchProducts(searchText , page);
            return Ok(result);
        }

        [HttpGet]
        [Route("searchsuggestions/{searchText}")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetProductSearchSuggestions(string searchText)
        {
            var result = await _productServices.GetProductSearchSuggestions(searchText);
            return Ok(result);
        }

        [HttpGet]
        [Route("featured")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetFeaturedProducts()
        {
            var result = await _productServices.GetFeaturedProducts();
            return Ok(result);
        }
    }
}
