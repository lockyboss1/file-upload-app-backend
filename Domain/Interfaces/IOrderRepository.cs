using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task AddOrdersAsync(IEnumerable<Order> orders);
    }
}
