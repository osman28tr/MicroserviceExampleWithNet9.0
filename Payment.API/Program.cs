using MassTransit;
using Payment.API.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMassTransit(configurator =>
{
	configurator.AddConsumer<StockReservedEventConsumer>();
	configurator.UsingRabbitMq((_context, _configurator) =>
	{
		_configurator.Host(builder.Configuration["RabbitMQ"]);

		_configurator.ReceiveEndpoint(RabbitMQSettings.Payment_StockReservedEventQueue, e => e.ConfigureConsumer<StockReservedEventConsumer>(_context));
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Run();
