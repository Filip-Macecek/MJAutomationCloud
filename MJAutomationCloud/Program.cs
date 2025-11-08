using MJAutomationCloud.Components;
using MJAutomationCloud.Infrastructure;
using MJAutomationCloud.Application;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using MJAutomationCloud.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container following DDD architecture

// Infrastructure layer services (Data, Identity, External services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure ASP.NET Core Identity (must be in ASP.NET Core project)
builder.Services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
{
    // Configure Identity options using our domain configuration
    MJAutomationCloud.Infrastructure.Identity.IdentityConfiguration.ConfigureIdentityOptions(options);
})
.AddEntityFrameworkStores<MJAutomationCloud.Infrastructure.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add custom password validator
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.IPasswordValidator<ApplicationUser>, MJAutomationCloud.Application.Services.PasswordStrengthValidator<ApplicationUser>>();

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie security settings
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // 8-hour session timeout
    options.SlidingExpiration = true; // Extend session on activity

    // Authentication paths
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Return URL parameter
    options.ReturnUrlParameter = "returnUrl";

    // Cookie name for identification
    options.Cookie.Name = "MJAutomationCloud.Auth";
});

// Application layer services (Business logic, Use cases)
builder.Services.AddApplication();

// Presentation layer services (Blazor components, UI)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure authentication state provider for Blazor Server
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Add cascade authentication state for Blazor components
builder.Services.AddCascadingAuthenticationState();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Ensure database is created and seeded (Development/Staging only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            // Apply database migrations
            await MJAutomationCloud.Infrastructure.DependencyInjection
                .EnsureDatabaseCreatedAsync(scope.ServiceProvider);

            // Seed initial data (admin user)
            await MJAutomationCloud.Infrastructure.DependencyInjection
                .SeedInitialDataAsync(scope.ServiceProvider, builder.Configuration);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Authentication and authorization middleware (order is important)
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Health checks endpoint
app.MapHealthChecks("/health");

// Map Razor components with authentication support
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();