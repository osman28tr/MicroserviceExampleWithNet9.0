using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using StockAPI.Entities;
using StockAPI.Services;

namespace StockAPI.Consumers
{
	public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
	{
		private readonly IMongoCollection<Stock> _stockCollection;
		private readonly ISendEndpointProvider _sendEndpointProvider;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
		{
			_stockCollection = mongoDbService.GetCollection<Stock>();
			_sendEndpointProvider = sendEndpointProvider;
			_publishEndpoint = publishEndpoint;
		}

		public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
		{
			List<bool> stockResult = new();

			context.Message.OrderItems.ForEach(async orderItem =>
			{
				var status = await _stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId && x.Quantity >= orderItem.Count).Result.AnyAsync();
				stockResult.Add(status);
			});

			if (!stockResult.TrueForAll(x => x.Equals(true)))
			{
				StockNotReservedEvent stockNotReservedEvent = new()
				{
					BuyerId = context.Message.BuyerId,
					OrderId = context.Message.OrderId,
					Message = "..."
				};

				await _publishEndpoint.Publish(stockNotReservedEvent);

				Console.WriteLine("Stok işlemleri başarısız");
			}
			else
			{
				context.Message.OrderItems.ForEach(async orderItem =>
				{
					var update = Builders<Stock>.Update.Inc(x => x.Quantity, -orderItem.Count);
					await _stockCollection.UpdateOneAsync(x => x.ProductId == orderItem.ProductId, update);
				});
				StockReservedEvent stockReservedEvent = new()
				{
					BuyerId = context.Message.BuyerId,
					OrderId = context.Message.OrderId,
					TotalPrice = context.Message.TotalPrice
				};

				ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
				await sendEndpoint.Send(stockReservedEvent);

				Console.WriteLine("Stok işlemleri başarılı");
			}				
		}
	}
}
