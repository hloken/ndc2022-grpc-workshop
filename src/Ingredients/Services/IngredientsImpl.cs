using System.Diagnostics;
using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;

namespace Ingredients.Services;

public class IngredientsImpl : IngredientsService.IngredientsServiceBase
{
    private readonly IToppingData _toppingData;
    private readonly ICrustData _crustData;
    private readonly ILogger<IngredientsImpl> _logger;

    public IngredientsImpl(IToppingData toppingData, ICrustData crustData, ILogger<IngredientsImpl> logger)
    {
        _toppingData = toppingData;
        _crustData = crustData;
        _logger = logger;
    }

    public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var toppings = await _toppingData.GetAsync(context.CancellationToken);

        var response = new GetToppingsResponse
        {
            Toppings =
            {
                toppings.OrderBy(t => t.Id)
                    .Select(t => new Topping
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Price = t.Price
                    })
            }
        };

        var stop = Stopwatch.GetTimestamp();
        
        _logger.LogInformation("GetToppings completed in {Duration}", (stop - start).ToString());
        return response;
    }

    public override async Task<GetCrustsResponse> GetCrusts(GetCrustsRequest request, ServerCallContext context)
    {
        var crusts = await _crustData.GetAsync(context.CancellationToken);

        var response = new GetCrustsResponse
        {
            Crusts =
            {
                crusts.OrderBy(c => c.Id)
                    .Select(c => new Crust
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Price = c.Price,
                        Size = c.Size,
                        StockCount = c.StockCount
                    })
            }
        };

        return response;
    }

    public override async Task<DecrementToppingsResponse> DecrementToppings(DecrementToppingsRequest request,
        ServerCallContext context)
    {
        var tasks = request.ToppingIds
            .Select(id => _toppingData.DecrementStockAsync(id, context.CancellationToken));

        await Task.WhenAll(tasks);
        return new DecrementToppingsResponse();
    }
    
    public override async Task<DecrementCrustsResponse> DecrementCrusts(DecrementCrustsRequest request,
        ServerCallContext context)
    {
        var tasks = request.CrustIds
            .Select(id => _toppingData.DecrementStockAsync(id, context.CancellationToken));

        await Task.WhenAll(tasks);
        return new DecrementCrustsResponse();
    }
}