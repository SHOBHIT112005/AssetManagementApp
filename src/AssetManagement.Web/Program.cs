using AssetManagement.Application.Interfaces;
using AssetManagement.Infrastructure.Repositories;
using AssetManagement.Web.Components;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Application.Services;
using AssetManagement.Web.Components.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContext<AssetDbContext>(
    options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )    
);
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
