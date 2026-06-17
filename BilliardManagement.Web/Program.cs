using BilliardManagement.Web.Services;
using BilliardManagement.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure HttpContextAccessor for Session and Token management
builder.Services.AddHttpContextAccessor();

// Configure Session with 30-minute timeout
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".BilliardMgmt.Session";
});

// Configure HttpClient
builder.Services.AddHttpClient("Api", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseAddress"] ?? "http://localhost:8080/api/";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register API Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<RevenueService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<StaffService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use Session BEFORE middleware that reads it
app.UseSession();

// Custom Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuthMiddleware>();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
