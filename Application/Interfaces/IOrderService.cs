using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(string id);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
        Task AddOrderAsync(OrderDto orderDto);
        Task UpdateOrderAsync(string id, OrderDto orderDto);
        Task DeleteOrderAsync(string id);
    }
}

