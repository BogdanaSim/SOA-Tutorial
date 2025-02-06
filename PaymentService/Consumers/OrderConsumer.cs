using MassTransit;

namespace PaymentService.Consumers;

public class OrderConsumer : IConsumer<Order>
{
	public async Task Consume(ConsumeContext<Order> context)
	{
		var order = context.Message;
		Console.WriteLine($"[x] Processing payment for Order ID: {order.Id}, Product: {order.Product}");
		await Task.CompletedTask;
	}
}

public record Order(int Id, string Product, decimal Price);