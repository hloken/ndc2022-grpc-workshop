using Orders.Proto;
using Shop;

var macOs = OperatingSystem.IsMacOS();
var binding = macOs ? "http" : "https";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var defaultOrdersUri = macOs ? "http://localhost:5004" : "https://localhost:5005";
        
        services.AddGrpcClient<OrderService.OrderServiceClient>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var ordersUri = configuration.GetServiceUri("orders", binding)
                            ?? new Uri(defaultOrdersUri);
            options.Address = ordersUri;
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
