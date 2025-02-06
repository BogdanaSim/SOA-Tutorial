using MassTransit;
using PaymentService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(config =>
{
	config.AddConsumer<OrderConsumer>();

	config.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host("localhost", "/", h =>
		{
			h.Username("guest");
			h.Password("guest");
		});

		cfg.ReceiveEndpoint("orderQueue", ep =>
		{
			ep.ConfigureConsumer<OrderConsumer>(context);
		});
	});
});

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();