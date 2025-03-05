using System.Globalization;
using Application.DTOs;
using Application.Exceptions;
using Application.Helpers;
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
            var recordErrors = new List<OrderValidationError>();

            await foreach (var record in csv.GetRecordsAsync<OrderDto>())
            {
                var errorsForRecord = new List<string>();

                // Required field validations
                ValidateRequiredField(record.OrderNumber, "OrderNumber", errorsForRecord);
                ValidateRequiredField(record.ShipToName, "ShipToName", errorsForRecord);
                ValidateRequiredField(record.ShipToAddress1, "ShipToAddress1", errorsForRecord);
                ValidateRequiredField(record.ShipToCity, "ShipToCity", errorsForRecord);
                ValidateRequiredField(record.ShipToState, "ShipToState", errorsForRecord);
                ValidateRequiredField(record.ShipToPostalCode, "ShipToPostalCode", errorsForRecord);
                ValidateRequiredField(record.ShipToCountry, "ShipToCountry", errorsForRecord);
                ValidateRequiredField(record.Sku, "Sku", errorsForRecord);
                ValidateRequiredField(record.RequestedWarehouse, "RequestedWarehouse", errorsForRecord);
                ValidateRequiredField(record.Quantity, "Quantity", errorsForRecord);

                // Check for duplicate OrderNumber within the file
                if (orders.Any(o => o.OrderNumber == record.OrderNumber))
                    errorsForRecord.Add($"Duplicate OrderNumber '{record.OrderNumber}' found in file.");

                // Check for duplicate OrderNumber in the database
                var existingOrder = await _orderRepository.GetByOrderNumberAsync(record.OrderNumber);
                if (existingOrder != null)
                    errorsForRecord.Add($"OrderNumber '{record.OrderNumber}' already exists in the database.");

                // Date format validation (allowing multiple formats)
                DateTime parsedOrderDate = default;
                if (!string.IsNullOrWhiteSpace(record.OrderDate))
                {
                    string[] allowedFormats = { "M/d/yyyy", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy" };
                    if (!DateTime.TryParseExact(record.OrderDate, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedOrderDate))
                    {
                        errorsForRecord.Add("OrderDate must be in a valid format (e.g., MM/dd/yyyy or M/d/yyyy).");
                    }
                }

                // Quantity validation: must be a positive integer
                if (!int.TryParse(record.Quantity.ToString(), out int quantity) || quantity <= 0)
                    errorsForRecord.Add("Quantity must be a positive integer.");

                // Postal Code validation: must be an integer and exactly 5 digits long
                if (!int.TryParse(record.ShipToPostalCode, out _) || record.ShipToPostalCode.Length != 5)
                    errorsForRecord.Add("ShipToPostalCode must be a 5-digit integer.");

                // ShipToState validation: must be exactly two characters (state abbreviation)
                if (record.ShipToState?.Length != 2)
                    errorsForRecord.Add("ShipToState must be a two-character state abbreviation (e.g, PA).");

                // Accumulate errors if any validations failed for this record.
                if (errorsForRecord.Any())
                {
                    // Add structured error for this record
                    recordErrors.Add(new OrderValidationError
                    {
                        OrderNumber = record.OrderNumber,
                        Messages = errorsForRecord
                    });
                }
                else
                {
                    // If all validations pass, add the order
                    orders.Add(new Order
                    {
                        OrderNumber = record.OrderNumber,
                        AlternateOrderNumber = record.AlternateOrderNumber,
                        OrderDate = string.IsNullOrWhiteSpace(record.OrderDate) ? null : parsedOrderDate.ToString("MM/dd/yyyy"),
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
                        Quantity = 3,
                        RequestedWarehouse = record.RequestedWarehouse,
                        DeliveryInstructions = record.DeliveryInstructions,
                        Tags = record.Tags
                    });
                }                
            }

            // If there are any validation errors, throw an exception and do not save anything
            if (recordErrors.Any())
            {
                throw new CustomValidationException(recordErrors);
            }

            // Save valid orders to the database only if no validation errors exist
            if (orders.Any())
            {
                await _orderRepository.AddOrdersAsync(orders);
            }
        }

        private void ValidateRequiredField(string fieldValue, string fieldName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                errors.Add($"{fieldName} is required.");
        }
    }
}

