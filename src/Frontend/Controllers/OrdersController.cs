using Frontend.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Orders.Protos;

namespace Frontend.Controllers;

[Route("orders")]
public class OrdersController : Controller
{
    private readonly OrderService.OrderServiceClient _orderServiceClient;
    private readonly ILogger<OrdersController> _log;

    public OrdersController(ILogger<OrdersController> log, OrderService.OrderServiceClient orderServiceClient)
    {
        _log = log;
        _orderServiceClient = orderServiceClient;
    }

    [HttpPost]
    public async Task<ActionResult> Order([FromForm]HomeViewModel viewModel)
    {
        var placeOrderRequest = new PlaceOrderRequest
        {
            ToppingsIds =
            {
                viewModel.Toppings
                    .Where(t => t.Selected)
                    .Select(t => t.Id)
            },
            CrustId = viewModel.SelectedCrust
        };

        try
        {
            var response = await _orderServiceClient.PlaceOrderAsync(placeOrderRequest);
            ViewData["DueBy"] = response.DueBy.ToDateTimeOffset();
            return View();
        }
        catch (RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            Console.WriteLine(ex);
            throw;
        }

    }
    
    
}