using AssetManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/account");

        group.MapPost("/login", async (
            [FromForm] string userName,
            [FromForm] string password,
            [FromForm] bool? rememberMe,
            [FromQuery] string? returnUrl,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var result = await signInManager.PasswordSignInAsync(
                userName, password, rememberMe ?? false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return Results.LocalRedirect(returnUrl ?? "/");
            }

            if (result.IsLockedOut)
                return Results.Redirect("/login?error=locked");

            return Results.Redirect("/login?error=invalid");
        });

        group.MapPost("/logout", async (
            SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/login");
        });
    }
}
