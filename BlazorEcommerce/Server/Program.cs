global using BlazorEcommerce.Server.Data;
global using BlazorEcommerce.Server.Services.AddressService;
global using BlazorEcommerce.Server.Services.AuthService;
global using BlazorEcommerce.Server.Services.CartService;
global using BlazorEcommerce.Server.Services.CategoryService;
global using BlazorEcommerce.Server.Services.OrderService;
global using BlazorEcommerce.Server.Services.PaymentService;
global using BlazorEcommerce.Server.Services.ProductService;
global using BlazorEcommerce.Server.Services.ProductTypeService;
global using BlazorEcommerce.Shared;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer(); // swagger 1
builder.Services.AddSwaggerGen(); //swagger 2

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); 
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(obtions =>
    {
        obtions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// ตัวเข้าถึงบริบท
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

app.UseSwaggerUI();//swagger 3

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();//swagger 4
app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//------- ลงทะเบียนความปลอดภัย -------
app.UseAuthentication();
app.UseAuthorization();
//-------------------------------

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
