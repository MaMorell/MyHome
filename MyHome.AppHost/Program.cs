var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var tibberAccessToken = builder.AddParameter("TibberApiClientAccessToken", secret: true);
var ebecoPassword = builder.AddParameter("ThermostatEbecoPassword", secret: true);
var homeAssistantAccessToken = builder.AddParameter("HomeAssistantAccessToken", secret: true);

var apiService = builder
    .AddDotnetProject("myhome-api", Path.Combine("..", "MyHome.ApiService", "MyHome.ApiService.csproj"))
    .WithEnvironment("TibberApiClient__AccessToken", tibberAccessToken)
    .WithEnvironment("ThermostatEbeco__Password", ebecoPassword)
    .WithEnvironment("HomeAssistant__AccessToken", homeAssistantAccessToken)
    .WithEnvironment("TZ", "Europe/Stockholm");

builder.AddDotnetProject("myhome-web", Path.Combine("..", "MyHome.Web", "MyHome.Web.csproj"))
    .WithReference(apiService)
    .WithExternalHttpEndpoints()
    .WithEnvironment("TZ", "Europe/Stockholm");

builder.Build().Run();
