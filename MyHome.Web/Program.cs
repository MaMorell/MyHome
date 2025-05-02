using MudBlazor.Services;
using MyHome.ServiceDefaults;
using MyHome.Web;
using MyHome.Web.Components;
using MyHome.Web.ExternalClients;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

var apiServiceConfig = builder.Configuration.GetRequiredSection(ApiServiceOptions.Name);
builder.Services
    .Configure<ApiServiceOptions>(apiServiceConfig)
    .AddOptionsWithValidateOnStart<ApiServiceOptions>()
    .ValidateDataAnnotations();

builder.Services.AddScoped<ApiServiceClient>();

builder.Services
    .AddScoped<EnergySupplierClient>()
    .AddScoped<AuditClient>()
    .AddScoped<PriceThearsholdsClient>()
    .AddScoped<DeviceSettingsProfileClient>()
    .AddScoped<WifiSocketClient>();

builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
