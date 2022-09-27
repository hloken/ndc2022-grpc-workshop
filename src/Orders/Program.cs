using Ingredients.Protos;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Orders.PubSub;
using Orders.Services;

var runningInContainer = "true".Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));
var macOs = OperatingSystem.IsMacOS();

var builder = WebApplication.CreateBuilder(args);

if (runningInContainer)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http2);
    });
}

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var defaultIngredientsUri = macOs ? "http://localhost:5002" : "https://localhost:5003";
var binding = macOs ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding)
                     ?? new Uri(defaultIngredientsUri);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddOrderPubSub();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
