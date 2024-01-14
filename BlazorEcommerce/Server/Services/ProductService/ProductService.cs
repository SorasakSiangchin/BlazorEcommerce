namespace BlazorEcommerce.Server.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(DataContext context , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<Product>> CreateProduct(Product product)
        {
            foreach (var variants in product.Variants)
            {
                variants.ProductType = null;
            }
            _context.Add(product);
            await _context.SaveChangesAsync();

            return new ServiceResponse<Product> { Data = product };
        }

        public async Task<ServiceResponse<bool>> DeleteProduct(int productId)
        {
            var dbProduct = await _context.Products.FindAsync(productId);
            if(dbProduct == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Product not found.",
                };
            }
            dbProduct.Deleted = true;
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool>
            {
                Data = true,
            };
        }

        public async Task<ServiceResponse<List<Product>>> GetAdminProducts()
        {
            var response = new ServiceResponse<List<Product>>
            {
                Data = await _context.Products.Where(p => !p.Deleted)
               .Include(e => e.Variants.Where(v => !v.Deleted))
               .ThenInclude(e => e.ProductType)
               .Include(p => p.Images)
               .ToListAsync()
            };

            return response;
        }

        public async Task<ServiceResponse<List<Product>>> GetFeaturedProducts()
        {
            var response = new ServiceResponse<List<Product>>
            {
                Data = await _context.Products.Where(e => e.Featured && e.Visible && !e.Deleted)
                .Include(e => e.Variants.Where(v => v.Visible && !v.Deleted))
                .ThenInclude(e => e.ProductType)
                  .Include(p => p.Images)
                .ToListAsync()
            };

            return response;
        }

        public async Task<ServiceResponse<Product>> GetProductAsync(int productId)
        {
            var response = new ServiceResponse<Product>();
            Product product = null;
            if (_httpContextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                product = await _context.Products
                    .Include(e => e.Variants.Where(v => !v.Deleted)) // เอาที่มองเห็นได้ และไม่ได้ลบ
                    .ThenInclude(e => e.ProductType)
                      .Include(p => p.Images)
                    .FirstOrDefaultAsync(e => e.Id == productId && !e.Deleted);
            }
            else
            {
                product = await _context.Products
                     .Include(e => e.Variants.Where(v => v.Visible && !v.Deleted)) // เอาที่มองเห็นได้ และไม่ได้ลบ
                     .ThenInclude(e => e.ProductType)
                       .Include(p => p.Images)
                     .FirstOrDefaultAsync(e => e.Id == productId && !e.Deleted && e.Visible);
            }

            if (product == null) 
            {
                response.Success = false;
                response.Message = "Sorry, but this product does not exist.";
            }
            else
            {
                response.Data = product;
            }
            return response;
        }

        public async Task<ServiceResponse<List<Product>>> GetProductsAsync()
        {
            var response = new ServiceResponse<List<Product>> { 
                Data = await _context.Products
                .Where(p => p.Visible && !p.Deleted)
                .Include(e => e.Variants.Where(v => v.Visible && !v.Deleted))
                .ThenInclude(e => e.ProductType)
                  .Include(p => p.Images)
                .ToListAsync()
            };

            return response;
        }

        public async Task<ServiceResponse<List<Product>>> GetProductsByCategory(string categoryUrl)
        {
            var response = new ServiceResponse<List<Product>>
            {
                Data = await _context.Products 
                .Include(e => e.Variants.Where(v => v.Visible && !v.Deleted))
                .ThenInclude(e => e.ProductType)
                .Include(p => p.Images)
                .Where(e => e.Category.Url.ToLower().Equals(categoryUrl.ToLower()) && e.Visible && !e.Deleted)
                .ToListAsync(),
            };

            return response;
        }

        public async Task<ServiceResponse<List<string>>> GetProductSearchSuggestions(string searchText)
        {
            var products = await FindProductsBySearchText(searchText);

            List<string> result = new();

            foreach (var product in products)
            {
                // StringComparison คือ การเปรียบเทียบ string
                if (product.Title.Contains(searchText , StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(product.Title);
                }

                // เป้าหมายคือเพื่อนำคำที่ค้นหามาค้นหาใน Description ว่ามีคำตรงกันหรือป่าว
                if (product.Description is not null)
                {

                    // Where(char.IsPunctuation) ค้นหาเครื่องหมาย , . \ ; ] [ ) (
                    // Distinct คือ การแสดงข้อมูลโดยไม่ซ้ำกัน
                    var punctuation = product.Description.Where(char.IsPunctuation)
                        .Distinct().ToArray();

                    // Trim เป็นการตัดช่องว่างทิ้ง
                    // Split แบ่งเป้น array
                    var words = product.Description.Split().Select(s => s.Trim(punctuation));

                    foreach (var word in words)
                    {
                        if (word.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !result.Contains(word))
                        {
                            result.Add(word);
                        }
                    }
                }
            }

            return new ServiceResponse<List<string>>
            {
                Data = result
            };
        }

        public async Task<ServiceResponse<ProductSearchResult>> SearchProducts(string searchText , int page)
        {
            var pageResults = 2f;
            // Ceiling ปัดเศษขึ้น
            // pageCount เก็บจำนวนหน้า
            var pageCount = Math.Ceiling((await FindProductsBySearchText(searchText)).Count/ pageResults);
            var products = await _context.Products.Include(e => e.Variants)
                                                  .Include(p => p.Images)
                                .Where(e => e.Title.ToLower().Contains(searchText.ToLower()) ||
                                     e.Description.ToLower().Contains(searchText.ToLower()) &&
                                     e.Visible && !e.Deleted)
                                .Skip((page - 1) * (int)pageResults) // จะข้ามไปที่ละหน้า
                                .Take((int)pageResults) // จะให้แสดงเท่าไร
                                .ToListAsync();
            var response = new ServiceResponse<ProductSearchResult>
            {
                Data = new ProductSearchResult
                {
                    Products = products,
                    CurrentPage = page ,
                    Page = (int)pageCount
                }
            };
            return response;
        }

        public async Task<ServiceResponse<Product>> UpdateProduct(Product product)
        {
            var dbProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (dbProduct == null)
            {
                return new ServiceResponse<Product>
                {
                    Success = false,
                    Message = "Product not found.",
                };
            }

            dbProduct.Title = product.Title;
            dbProduct.Description = product.Description;
            dbProduct.Visible = product.Visible;
            dbProduct.ImageUrl= product.ImageUrl;
            dbProduct.CategoryId = product.CategoryId;
            dbProduct.Featured = product.Featured;

            // ลบรูปภาพอันเก่าทิ้งทั้งหมด แล้วเอาอันใหม่ มาแทน
            var productImage = dbProduct.Images;
            _context.Images.RemoveRange(productImage);
            dbProduct.Images = product.Images;

            foreach (var variant in product.Variants)
            {
                var dbVariant = await _context.ProductVariants
                    .SingleOrDefaultAsync(v => v.ProductId == variant.ProductId && 
                    v.ProductTypeId == variant.ProductTypeId);

                if (dbVariant == null)
                {
                    variant.ProductType = null; 
                    _context.ProductVariants.Add(variant);
                }
                else
                {
                    dbVariant.ProductTypeId = variant.ProductTypeId;
                    dbVariant.Price = variant.Price;
                    dbVariant.OriginalPrice = variant.OriginalPrice;
                    dbVariant.Visible= variant.Visible;
                    dbVariant.Deleted= variant.Deleted;
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResponse<Product>
            {
                Data = product
            };
        }

        private async Task<List<Product>> FindProductsBySearchText(string searchText)
        {
            return await _context.Products.Include(e => e.Variants)
                                            .Include(p => p.Images).Where(e =>
                                e.Title.ToLower().Contains(searchText.ToLower()) ||
                                e.Description.ToLower().Contains(searchText.ToLower()) && 
                                e.Visible && !e.Deleted).ToListAsync();
        }
    }
}
