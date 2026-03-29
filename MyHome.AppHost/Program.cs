var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var tibberAccessToken = builder.AddParameter("TibberApiClientAccessToken", secret: true);
var upLinkSecret = builder.AddParameter("UpLinkOptionsClientSecret", secret: true);
var ebecoPassword = builder.AddParameter("ThermostatEbecoPassword", secret: true);

var apiService = builder
    .AddProject<Projects.MyHome_ApiService>("myhome-api")
    .WithEnvironment("UpLinkOptions__ClientSecret", upLinkSecret)
    .WithEnvironment("TibberApiClient__AccessToken", tibberAccessToken)
    .WithEnvironment("ThermostatEbeco__Password", ebecoPassword);

builder.AddProject<Projects.MyHome_Web>("myhome-web")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
