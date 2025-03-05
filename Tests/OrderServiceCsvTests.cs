using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using System.Text;
using Xunit;

namespace Tests
{
    public class OrderServiceCsvTests
    {
        [Fact]
        public async Task AddInvalidCsvOrder()
        {
            // Arrange: create a CSV with an invalid record (e.g. missing ShipToName).
            var csvContent = new StringBuilder();
            csvContent.AppendLine("OrderNumber,ShipToName,ShipToAddress1,ShipToCity,ShipToState,ShipToPostalCode,ShipToCountry,Sku,Quantity,RequestedWarehouse,OrderDate");
            csvContent.AppendLine("ORDER1,,123 Main St,City,PA,12345,USA,SKU1,10,WH1,01/03/2025");

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));
            IFormFile file = new FormFile(stream, 0, stream.Length, "csv", "test.csv");

            // Set up repository mock: return null (meaning no duplicate in db) for ORDER1.
            var mockRepo = new Mock<IOrderRepository>();
            mockRepo.Setup(repo => repo.GetByOrderNumberAsync("ORDER1"))
                    .ReturnsAsync((Order)null);

            var orderService = new OrderService(mockRepo.Object);
            var ex = await Assert.ThrowsAsync<CustomValidationException>(() => orderService.AddOrderAsync(file));
            var combinedErrors = string.Join(" ", ex.Errors.SelectMany(e => e.Messages));
            Assert.Contains("ShipToName is required", combinedErrors);
        }

        [Fact]
        public async Task AddValidCsvOrder()
        {
            // Arrange: create valid CSV content.
            var csvContent = new StringBuilder();
            csvContent.AppendLine("OrderNumber,ShipToName,ShipToAddress1,ShipToCity,ShipToState,ShipToPostalCode,ShipToCountry,Sku,Quantity,RequestedWarehouse,OrderDate");
            csvContent.AppendLine("ORDER2,John Doe,123 Main St,City,PA,12345,USA,SKU1,10,WH1,01/03/2025");

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));
            IFormFile file = new FormFile(stream, 0, stream.Length, "csv", "test.csv");

            // Set up repository mock: return null for ORDER2 (no duplicate).
            var mockRepo = new Mock<IOrderRepository>();
            mockRepo.Setup(repo => repo.GetByOrderNumberAsync("ORDER2"))
                    .ReturnsAsync((Order)null);
            mockRepo.Setup(repo => repo.AddOrdersAsync(It.IsAny<IEnumerable<Order>>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var orderService = new OrderService(mockRepo.Object);

            // Act
            await orderService.AddOrderAsync(file);

            // Assert that AddOrdersAsync was called exactly once.
            mockRepo.Verify(repo => repo.AddOrdersAsync(It.IsAny<IEnumerable<Order>>()), Times.Once);
        }
    }
}
