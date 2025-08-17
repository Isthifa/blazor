var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.BlazorWebPoc_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BlazorWebPoc_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
