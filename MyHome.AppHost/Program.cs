var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var tibberAccessToken = builder.AddParameter("TibberApiClientAccessToken", secret: true);
var ebecoPassword = builder.AddParameter("ThermostatEbecoPassword", secret: true);

var apiService = builder
    .AddProject<Projects.MyHome_ApiService>("myhome-api")
    .WithEnvironment("TibberApiClient__AccessToken", tibberAccessToken)
    .WithEnvironment("ThermostatEbeco__Password", ebecoPassword)
    .WithEnvironment("TZ", "Europe/Stockholm"); ;

builder.AddProject<Projects.MyHome_Web>("myhome-web")
    .WithEndpoint("http", endpoint =>
    {
        endpoint.Port = 5001;
    })
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
