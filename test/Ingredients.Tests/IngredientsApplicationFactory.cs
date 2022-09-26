using Grpc.Net.Client;
using Ingredients.Data;
using Ingredients.Protos;
using Ingredients.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Ingredients.Tests;

public class IngredientsApplicationFactory : WebApplicationFactory<IngredientsImpl>
{
    public IngredientsService.IngredientsServiceClient CreateGrpcClient()
    {
        var httpClient = CreateClient();

        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        return new IngredientsService.IngredientsServiceClient(channel);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(s =>
        {
            SubToppingData(s);
            SubCrustData(s);
        });
        
        base.ConfigureWebHost(builder);
    }

    private static void SubToppingData(IServiceCollection services)
    {
        services.RemoveAll<IToppingData>();

        var list = new List<ToppingEntity>
        {
            new("cheese", "Cheese", 1d, 10),
            new("tomato", "Tomato", 0.5d, 10)
        };

        var sub = Substitute.For<IToppingData>();
        sub.GetAsync(Arg.Any<CancellationToken>())
            .Returns(list);

        services.AddSingleton(sub);
    }

    private static void SubCrustData(IServiceCollection services)
    {
        services.RemoveAll<ICrustData>();

        var list = new List<CrustEntity>
        {
            new("deepdish", "Deep Dish", 2, 30d, 15),
            new("italian", "Italien Style", 1, 10d, 10)
        };

        var sub = Substitute.For<ICrustData>();
        sub.GetAsync(Arg.Any<CancellationToken>())
            .Returns(list);

        services.AddSingleton(sub);
    }
}