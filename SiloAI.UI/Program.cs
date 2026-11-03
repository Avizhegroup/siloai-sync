using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SiloAI.Identity.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; })
    .AddHubOptions(options =>
    {
     
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
    });

builder.Services.AddRazorPages();

builder.Services.AddAuthenticationCore();

// Admin-only policy. The JWT's role claim reaches the UI as a raw "role" claim, which a
// ClaimsIdentity built with its default role type does not treat as a role, so a plain
// IsInRole/Roles= check would reject every admin; accept both claim types explicitly.
builder.Services.AddAuthorizationCore(options =>
    options.AddPolicy(SiloAI.UI.Services.UiPolicies.AdminOnly, policy => policy.RequireAssertion(ctx =>
        ctx.User.IsInRole("Admin") ||
        ctx.User.Claims.Any(c =>
            (c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role) && c.Value == "Admin"))));

builder.Services.AddOptions();

builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<AuthenticationStateProvider, SiloAuthenticationStateProvider>();

builder.Services.AddScoped(sp =>
    (SiloAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddScoped<IClaimManager, AiClaimManager>();

builder.Services.AddScoped<IAiAuthenticationService, AiAuthenticationService>();

builder.Services.AddScoped<AiApiClient>();

builder.Services.AddHttpClient("AiApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AiApi:BaseUrl"] ?? "http://localhost:5100/");
    // Image generation can take well over the 100s default.
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();
