using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
	public class StockReservedEventConsumer : IConsumer
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task Consume(ConsumeContext<StockReservedEvent> context)
		{
			//fake payment process

			if (true)
			{
				PaymentCompletedEvent paymentCompletedEvent = new()
				{
					OrderId = context.Message.OrderId
				};
				await _publishEndpoint.Publish(paymentCompletedEvent);

				Console.WriteLine("Ödeme başarılı");
			}
			else
			{
				PaymentFailedEvent paymentFailedEvent = new()
				{
					OrderId = context.Message.OrderId,
					Message = "Bakiye yetersiz"
				};
				Console.WriteLine("Ödeme başarısız");
				await _publishEndpoint.Publish(paymentFailedEvent);
			}
			
		}
	}
}
