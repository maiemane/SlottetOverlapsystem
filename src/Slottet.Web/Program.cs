using Slottet.Components;
using Slottet.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<AuthApiOptions>(builder.Configuration.GetSection(AuthApiOptions.SectionName));
var apiOptions = builder.Configuration.GetSection(AuthApiOptions.SectionName).Get<AuthApiOptions>()
                ?? throw new InvalidOperationException("Api configuration is missing.");

builder.Services.AddHttpClient("SlottetApi", client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
});
builder.Services.AddScoped<AuthService>();

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
