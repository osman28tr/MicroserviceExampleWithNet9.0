using MassTransit;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
	public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
	{
		private readonly OrderDbContext _dbContext;

		public PaymentCompletedEventConsumer(OrderDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
		{
			var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
			if (order != null)
			{
				order.OrderStatus = Models.Enums.OrderStatus.Completed;
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
