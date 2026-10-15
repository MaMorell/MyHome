var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var tibberAccessToken = builder.AddParameter("TibberApiClientAccessToken", secret: true);
var ebecoPassword = builder.AddParameter("ThermostatEbecoPassword", secret: true);
var homeAssistantAccessToken = builder.AddParameter("HomeAssistantAccessToken", secret: true);

var apiService = builder
    .AddProject<Projects.MyHome_ApiService>("myhome-api")
    .WithEnvironment("TibberApiClient__AccessToken", tibberAccessToken)
    .WithEnvironment("ThermostatEbeco__Password", ebecoPassword)
    .WithEnvironment("HomeAssistant__AccessToken", homeAssistantAccessToken)
    .WithEnvironment("TZ", "Europe/Stockholm");

builder.Build().Run();
