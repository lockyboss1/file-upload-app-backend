using Microsoft.Extensions.Configuration;
using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var mongoSettings = configuration.GetSection("MongoDbSettings");
            var connectionString = mongoSettings["ConnectionString"];
            var databaseName = mongoSettings["DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
    }
}

