using MongoDB.Bson.Serialization.Attributes;

namespace StockAPI.Entities
{
	public class Stock
	{
		[BsonId]
		[BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
		[BsonElement(Order = 0)]
		public Guid Id { get; set; }

		[BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
		[BsonElement(Order = 1)]
		public Guid ProductId { get; set; }
		[BsonElement(Order = 2)]
		//int type		
		public int Quantity { get; set; }
	}
}
