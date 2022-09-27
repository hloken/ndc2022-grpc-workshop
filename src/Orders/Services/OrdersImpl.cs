using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Orders.Proto;
using Orders.PubSub;

namespace Orders.Services;

public class OrdersImpl: OrderService.OrderServiceBase
{
    private readonly IngredientsService.IngredientsServiceClient _ingredientsServiceClient;
    private readonly IOrderPublisher _orderPublisher;
    private readonly IOrderMessages _orderMessages;

    public OrdersImpl(IngredientsService.IngredientsServiceClient ingredientsServiceClient, IOrderPublisher  orderPublisher, IOrderMessages orderMessages)
    {
        _ingredientsServiceClient = ingredientsServiceClient;
        _orderPublisher = orderPublisher;
        _orderMessages = orderMessages;
    }

    public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
    {
        var decrementToppingsRequest = new DecrementToppingsRequest
        {
            ToppingIds = {request.ToppingsIds}
        };
        await _ingredientsServiceClient.DecrementToppingsAsync(decrementToppingsRequest);

        var decrementCrustsRequest = new DecrementCrustsRequest
        {
            CrustIds = {request.CrustId}
        };
        await _ingredientsServiceClient.DecrementCrustsAsync(decrementCrustsRequest);

        var dueBy = DateTimeOffset.UtcNow.AddMinutes(45);
        
        await _orderPublisher.PublishOrder(request.CrustId, request.ToppingsIds, dueBy);
        
        return new PlaceOrderResponse
        {
            DueBy = DateTimeOffset.UtcNow.AddMinutes(45).ToTimestamp()
        };
    }

    public override async Task Subscribe(SubscriberRequest request, IServerStreamWriter<OrderNotification> responseStream, ServerCallContext context)
    {
        var cancellationToken = context.CancellationToken;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _orderMessages.ReadAsync(cancellationToken);
                var notification = new OrderNotification
                {
                    CrustId = message.CrustId,
                    ToppingsId = {message.ToppingIds},
                    DueBy = message.Time.ToTimestamp()
                };
                try
                {
                    await responseStream.WriteAsync(notification);
                }
                catch
                {
                    await _orderPublisher.PublishOrder(message.CrustId, message.ToppingIds, message.Time);
                }

            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}