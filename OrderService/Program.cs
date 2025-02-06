using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(config =>
{
	config.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host("localhost", "/", h =>
		{
			h.Username("guest");
			h.Password("guest");
		});
	});
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();