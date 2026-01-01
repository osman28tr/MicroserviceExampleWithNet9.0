using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StockAPI.Models;

namespace StockAPI.Services
{
	public class MongoDbService
	{
		private readonly IMongoDatabase _stockDatabase;
		public MongoDbService(IOptions<MongoDbOption> options)
		{
			MongoClient mongoClient = new(options.Value.ConnectionString);
			_stockDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);
		}

		public IMongoCollection<T> GetCollection<T>() => _stockDatabase.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
	}
}
