using MassTransit;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
	public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
	{
		private readonly OrderDbContext _dbContext;

		public PaymentFailedEventConsumer(OrderDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
		{
			var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
			if (order != null)
			{
				order.OrderStatus = Models.Enums.OrderStatus.Failed;
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
