using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LanAnhComputer.Web.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(manager =>
    {
        var apiAssembly = typeof(LanAnhComputer.API.Controllers.ChatbotController).Assembly;
        var partsToRemove = manager.ApplicationParts
            .Where(part => part.Name == apiAssembly.GetName().Name)
            .ToList();
        foreach (var part in partsToRemove)
        {
            manager.ApplicationParts.Remove(part);
        }
    });

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<ICheckoutService, CheckoutService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<IAccountService, AccountService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<IAdminInventoryService, AdminInventoryService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<ICategoryService, CategoryService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

builder.Services.AddHttpClient<IChatbotService, ChatbotService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7132/");
});

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Session phải nằm sau UseRouting
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
