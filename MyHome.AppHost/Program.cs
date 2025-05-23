var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyHome_ApiService>("apiservice");

builder.AddProject<Projects.MyHome_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
