using Grpc.Net.Client;
using Ingredients.Protos;
using Ingredients.Services;
using Microsoft.AspNetCore.Mvc.Testing;

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
}