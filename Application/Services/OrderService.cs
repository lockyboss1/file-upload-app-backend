using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync() =>
            await _orderRepository.GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(string id) =>
            await _orderRepository.GetByIdAsync(id);

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber) =>
            await _orderRepository.GetByOrderNumberAsync(orderNumber);

        public async Task AddOrderAsync(OrderDto orderDto)
        {
            var order = new Order
            {
                OrderNumber = orderDto.OrderNumber,
                AlternateOrderNumber = orderDto.AlternateOrderNumber,
                OrderDate = orderDto.OrderDate,
                ShipToName = orderDto.ShipToName,
                ShipToCompany = orderDto.ShipToCompany,
                ShipToAddress1 = orderDto.ShipToAddress1,
                ShipToAddress2 = orderDto.ShipToAddress2,
                ShipToAddress3 = orderDto.ShipToAddress3,
                ShipToCity = orderDto.ShipToCity,
                ShipToState = orderDto.ShipToState,
                ShipToPostalCode = orderDto.ShipToPostalCode,
                ShipToCountry = orderDto.ShipToCountry,
                ShipToPhone = orderDto.ShipToPhone,
                ShipToEmail = orderDto.ShipToEmail,
                Sku = orderDto.Sku,
                Quantity = orderDto.Quantity,
                RequestedWarehouse = orderDto.RequestedWarehouse,
                DeliveryInstructions = orderDto.DeliveryInstructions,
                Tags = orderDto.Tags
            };

            await _orderRepository.AddAsync(order);
        }

        public async Task UpdateOrderAsync(string id, OrderDto orderDto)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null) return;

            existingOrder.OrderNumber = orderDto.OrderNumber;
            existingOrder.AlternateOrderNumber = orderDto.AlternateOrderNumber;
            existingOrder.OrderDate = orderDto.OrderDate;
            existingOrder.ShipToName = orderDto.ShipToName;
            existingOrder.ShipToCompany = orderDto.ShipToCompany;
            existingOrder.ShipToAddress1 = orderDto.ShipToAddress1;
            existingOrder.ShipToAddress2 = orderDto.ShipToAddress2;
            existingOrder.ShipToAddress3 = orderDto.ShipToAddress3;
            existingOrder.ShipToCity = orderDto.ShipToCity;
            existingOrder.ShipToState = orderDto.ShipToState;
            existingOrder.ShipToPostalCode = orderDto.ShipToPostalCode;
            existingOrder.ShipToCountry = orderDto.ShipToCountry;
            existingOrder.ShipToPhone = orderDto.ShipToPhone;
            existingOrder.ShipToEmail = orderDto.ShipToEmail;
            existingOrder.Sku = orderDto.Sku;
            existingOrder.Quantity = orderDto.Quantity;
            existingOrder.RequestedWarehouse = orderDto.RequestedWarehouse;
            existingOrder.DeliveryInstructions = orderDto.DeliveryInstructions;
            existingOrder.Tags = orderDto.Tags;

            await _orderRepository.UpdateAsync(existingOrder);
        }

        public async Task DeleteOrderAsync(string id) =>
            await _orderRepository.DeleteAsync(id);
    }
}

