using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.ViewModels;
using Order.API.Models;
using System.Threading.Tasks;
using MassTransit;
using Shared.Events;

namespace Order.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly OrderDbContext _dbContext;
	    private	readonly IPublishEndpoint _publishEndpoint;
		public OrdersController(OrderDbContext dbContext, IPublishEndpoint publishEndpoint)
		{
			_dbContext = dbContext;
			_publishEndpoint = publishEndpoint;
		}
		[HttpPost]
		public async Task<IActionResult> CreateOrder(CreateOrderVM createOrder)
		{
			Order.API.Models.Entities.Order order = new()
			{
				BuyerId = createOrder.BuyerId,
				CreateDate = DateTime.Now,
				OrderStatus = Models.Enums.OrderStatus.Suspend
			};

			order.OrderItems = createOrder.OrderItems.Select(x => new Models.Entities.OrderItem
			{
				Count = x.Count,
				Price = x.Price,
				ProductId = x.ProductId,				
			}).ToList();

			order.TotalPrice = createOrder.OrderItems.Sum(x => x.Price * x.Count);

			await _dbContext.AddAsync(order);
			await _dbContext.SaveChangesAsync();

			OrderCreatedEvent orderCreatedEvent = new()
			{
				BuyerId = createOrder.BuyerId,
				OrderId = order.Id,
				TotalPrice = order.TotalPrice,
				OrderItems = order.OrderItems.Select(x => new Shared.Messages.OrderItemMessage
				{
					Count = x.Count,
					ProductId = x.ProductId
				}).ToList()
			};

			await _publishEndpoint.Publish(orderCreatedEvent);

			return Ok("Success");
		}
	}
}
