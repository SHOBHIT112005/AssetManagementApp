using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Infrastructure.Repositories;
using AssetManagement.Web.Components;
using AssetManagement.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AssetManagement.Infrastructure.Identity;
using AssetManagement.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);

// Add services to the container.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("AssetManagementApp");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// 1. Business Data Context
builder.Services.AddDbContext<AssetDbContext>(
    options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )    
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AssetDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/api/account/logout";
    options.AccessDeniedPath = "/login?error=denied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "AssetManagement.Auth";
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();


builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();
builder.Services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAssetDbContext>(provider => provider.GetRequiredService<AssetDbContext>());
builder.Services.AddScoped<AdminAgentService>();
builder.Services.AddScoped<IAdminAgentQueryRepository, AdminAgentQueryRepository>();
builder.Services.AddHttpClient("AgentClient", client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Agent write command registrations removed in favor of direct service execution



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAccountEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

