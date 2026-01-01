using MassTransit;
using MongoDB.Driver;
using Shared;
using StockAPI.Consumers;
using StockAPI.Entities;
using StockAPI.Models;
using StockAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
	configurator.AddConsumer<OrderCreatedEventConsumer>();
	configurator.UsingRabbitMq((_context, _configurator) =>
	{
		_configurator.Host(builder.Configuration["RabbitMQ"]);

		_configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(_context));
	});
});
//MongoDbOption 
builder.Services.Configure<MongoDbOption>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<MongoDbService>();

#region seed data to MongoDB
using IServiceScope serviceScope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDbService = serviceScope.ServiceProvider.GetRequiredService<MongoDbService>();
var collection = mongoDbService.GetCollection<Stock>();
if (!collection.FindSync(s => true).Any())
{
	collection.InsertMany(new List<Stock>()
	{
		new Stock(){ProductId=Guid.NewGuid(),Quantity=1000},
		new Stock(){ProductId=Guid.NewGuid(),Quantity=2000},
		new Stock(){ProductId=Guid.NewGuid(),Quantity=3000},
		new Stock(){ProductId=Guid.NewGuid(),Quantity=4000}
	});
}
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
