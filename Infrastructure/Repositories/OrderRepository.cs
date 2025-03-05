using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(MongoDbContext dbContext) : base(dbContext, "Orders")
        {
            _orders = dbContext.Orders;
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber) =>
            await _orders.Find(o => o.OrderNumber == orderNumber).FirstOrDefaultAsync();

        public async Task AddOrdersAsync(IEnumerable<Order> orders)
        {
            if (orders != null && orders.Any())
            {
                await _orders.InsertManyAsync(orders);
            }
        }
    }
}