using System.Globalization;
using Application.DTOs;
using Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

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

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber) =>
            await _orderRepository.GetByOrderNumberAsync(orderNumber);

        public async Task AddOrderAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Uploaded file is empty.");

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header?.Trim().Replace("*", "") // Remove asterisks
            });

            var orders = new List<Order>();

            await foreach (var record in csv.GetRecordsAsync<OrderDto>())
            {
                if (string.IsNullOrWhiteSpace(record.OrderNumber) ||
                    string.IsNullOrWhiteSpace(record.ShipToName) ||
                    string.IsNullOrWhiteSpace(record.ShipToAddress1) ||
                    string.IsNullOrWhiteSpace(record.ShipToCity) ||
                    string.IsNullOrWhiteSpace(record.ShipToState) ||
                    string.IsNullOrWhiteSpace(record.ShipToPostalCode) ||
                    string.IsNullOrWhiteSpace(record.ShipToCountry) ||
                    string.IsNullOrWhiteSpace(record.Sku) ||
                    record.Quantity <= 0 ||
                    string.IsNullOrWhiteSpace(record.RequestedWarehouse))
                {
                    continue; // Skip invalid records
                }

                orders.Add(new Order
                {
                    OrderNumber = record.OrderNumber,
                    AlternateOrderNumber = record.AlternateOrderNumber,
                    OrderDate = record.OrderDate,
                    ShipToName = record.ShipToName,
                    ShipToCompany = record.ShipToCompany,
                    ShipToAddress1 = record.ShipToAddress1,
                    ShipToAddress2 = record.ShipToAddress2,
                    ShipToAddress3 = record.ShipToAddress3,
                    ShipToCity = record.ShipToCity,
                    ShipToState = record.ShipToState,
                    ShipToPostalCode = record.ShipToPostalCode,
                    ShipToCountry = record.ShipToCountry,
                    ShipToPhone = record.ShipToPhone,
                    ShipToEmail = record.ShipToEmail,
                    Sku = record.Sku,
                    Quantity = record.Quantity,
                    RequestedWarehouse = record.RequestedWarehouse,
                    DeliveryInstructions = record.DeliveryInstructions,
                    Tags = record.Tags
                });
            }

            if (orders.Any())
            {
                await _orderRepository.AddOrdersAsync(orders);
            }
        }

        public async Task DeleteOrderAsync(string id) =>
            await _orderRepository.DeleteAsync(id);
    }
}

