using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
	private readonly IPublishEndpoint _publishEndpoint;

	public OrderController(IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	[HttpPost]
	public async Task<IActionResult> PlaceOrder([FromBody] Order order)
	{
		await _publishEndpoint.Publish(order);
		return Ok("Order placed successfully");
	}
}

public record Order(int Id, string Product, decimal Price);