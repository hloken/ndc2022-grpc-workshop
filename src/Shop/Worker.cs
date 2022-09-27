using Grpc.Core;
using Orders.Proto;

namespace Shop;

public class Worker : BackgroundService
{
    private readonly OrderService.OrderServiceClient _orderServiceClient;
    private readonly ILogger<Worker> _logger;
    private HashSet<string> _seen = new HashSet<string>();
    
    public Worker(ILogger<Worker> logger, OrderService.OrderServiceClient orderServiceClient)
    {
        _logger = logger;
        _orderServiceClient = orderServiceClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stream = _orderServiceClient.Subscribe(new SubscriberRequest());

                await foreach (var notification in stream.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    if (_seen.Add(notification.NotificationId))
                    {
                        _logger.LogInformation("Order: {CrustId} with {ToppingIds} due by {DueBy}",
                            notification.CrustId,
                            string.Join(", ", notification.ToppingsId),
                            notification.DueBy.ToDateTimeOffset().ToLocalTime().ToString("t"));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
