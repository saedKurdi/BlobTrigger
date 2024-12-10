using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure BlobServiceClient for dependency injection
string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
builder.Services.AddSingleton(new BlobServiceClient(storageConnectionString));

// Application Insights isn't enabled by default. Uncomment if needed.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
