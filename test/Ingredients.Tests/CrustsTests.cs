using Ingredients.Protos;

namespace Ingredients.Tests;

public class CrustsTests: IClassFixture<IngredientsApplicationFactory>
{
    private readonly IngredientsApplicationFactory _factory;

    public CrustsTests(IngredientsApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCrusts()
    {
        var client = _factory.CreateGrpcClient();
        var response = await client.GetCrustsAsync(new GetCrustsRequest());
        
        Assert.Collection(response.Crusts, t => 
            Assert.Equal("deepdish", t.Id), t => Assert.Equal("italian", t.Id));
    }
}