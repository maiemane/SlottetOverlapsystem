using Slottet.Components;
using Slottet.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Slottet.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<AuthApiOptions>(builder.Configuration.GetSection(AuthApiOptions.SectionName));
var apiOptions = builder.Configuration.GetSection(AuthApiOptions.SectionName).Get<AuthApiOptions>()
                ?? throw new InvalidOperationException("Api configuration is missing.");

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthSessionStore, BrowserSessionAuthStore>();
builder.Services.AddScoped<BearerTokenHandler>();
builder.Services.AddHealthChecks();
var connectionString = builder.Configuration.GetConnectionString("SlottetDb")
                      ?? throw new InvalidOperationException("Connection string 'SlottetDb' was not found.");
var dataProtectionApplicationName = builder.Configuration["DataProtection:ApplicationName"] ?? "Slottet";
var reverseProxyEnabled = builder.Configuration.GetValue("ReverseProxy:Enabled", false);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDataProtection()
    .SetApplicationName(dataProtectionApplicationName)
    .PersistKeysToDbContext<ApplicationDbContext>();
builder.Services.AddHttpClient("SlottetApi", client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
}).AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

await app.Services.EnsureDatabaseMigratedAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (reverseProxyEnabled)
{
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };

    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
