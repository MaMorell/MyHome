using MyHome.ServiceDefaults;
using MyHome.Web.Components;
using MyHome.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiService:BaseUrl"]
    ?? (builder.Environment.IsDevelopment() && !builder.Configuration.GetSection("services:myhome-api").Exists()
        ? "http://localhost:5599"
        : "http://myhome-api");

builder.Services.AddHttpClient<EnergyDashboardClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<DeviceSettingsClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
