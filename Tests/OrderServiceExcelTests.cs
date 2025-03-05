using Application.Exceptions;
using Application.Services;
using ClosedXML.Excel;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Xunit;

namespace Tests
{
	public class OrderServiceExcelTests
	{
        [Fact]
        public async Task AddInvalidExcelOrder()
        {
            // Arrange: Create an Excel file in memory with invalid data (missing ShipToName)
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Set headers (ensure they match what OrderService expects after trimming/replacing)
            worksheet.Cell(1, 1).Value = "OrderNumber";
            worksheet.Cell(1, 2).Value = "ShipToName";
            worksheet.Cell(1, 3).Value = "ShipToAddress1";
            worksheet.Cell(1, 4).Value = "ShipToCity";
            worksheet.Cell(1, 5).Value = "ShipToState";
            worksheet.Cell(1, 6).Value = "ShipToPostalCode";
            worksheet.Cell(1, 7).Value = "ShipToCountry";
            worksheet.Cell(1, 8).Value = "Sku";
            worksheet.Cell(1, 9).Value = "Quantity";
            worksheet.Cell(1, 10).Value = "RequestedWarehouse";
            worksheet.Cell(1, 11).Value = "OrderDate";

            // Add one invalid record: missing ShipToName.
            worksheet.Cell(2, 1).Value = "ORDER1";
            worksheet.Cell(2, 2).Value = "";              // Missing ShipToName
            worksheet.Cell(2, 3).Value = "123 Main St";
            worksheet.Cell(2, 4).Value = "City";
            worksheet.Cell(2, 5).Value = "PA";
            worksheet.Cell(2, 6).Value = "12345";
            worksheet.Cell(2, 7).Value = "USA";
            worksheet.Cell(2, 8).Value = "SKU1";
            worksheet.Cell(2, 9).Value = "10";
            worksheet.Cell(2, 10).Value = "WH1";
            worksheet.Cell(2, 11).Value = "01/03/2025"; 

            // Save workbook to a memory stream.
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            // Create an IFormFile for Excel.
            IFormFile file = new FormFile(stream, 0, stream.Length, "excel", "test.xlsx");

            // Set up repository mock: for ORDER1, no duplicate in the database.
            var mockRepo = new Mock<IOrderRepository>();
            mockRepo.Setup(repo => repo.GetByOrderNumberAsync("ORDER1"))
                    .ReturnsAsync((Order)null);

            // Create OrderService with the mocked repository.
            var orderService = new OrderService(mockRepo.Object);

            // Act & Assert: expect a CustomValidationException.
            var ex = await Assert.ThrowsAsync<CustomValidationException>(() => orderService.AddOrderAsync(file));

            // Optionally verify the error messages contain "ShipToName is required."
            var combinedErrors = string.Join(" ", ex.Errors.Select(e => string.Join(" ", e.Messages)));
            Assert.Contains("ShipToName is required", combinedErrors);
        }

        [Fact]
        public async Task AddValidExcelOrder()
        {
            // Arrange: Create an Excel file in memory with valid data.
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Set headers
            worksheet.Cell(1, 1).Value = "OrderNumber";
            worksheet.Cell(1, 2).Value = "ShipToName";
            worksheet.Cell(1, 3).Value = "ShipToAddress1";
            worksheet.Cell(1, 4).Value = "ShipToCity";
            worksheet.Cell(1, 5).Value = "ShipToState";
            worksheet.Cell(1, 6).Value = "ShipToPostalCode";
            worksheet.Cell(1, 7).Value = "ShipToCountry";
            worksheet.Cell(1, 8).Value = "Sku";
            worksheet.Cell(1, 9).Value = "Quantity";
            worksheet.Cell(1, 10).Value = "RequestedWarehouse";
            worksheet.Cell(1, 11).Value = "OrderDate";

            // Add a valid record.
            worksheet.Cell(2, 1).Value = "ORDER2";
            worksheet.Cell(2, 2).Value = "John Doe";
            worksheet.Cell(2, 3).Value = "123 Main St";
            worksheet.Cell(2, 4).Value = "City";
            worksheet.Cell(2, 5).Value = "PA";
            worksheet.Cell(2, 6).Value = "12345";
            worksheet.Cell(2, 7).Value = "USA";
            worksheet.Cell(2, 8).Value = "SKU1";
            worksheet.Cell(2, 9).Value = "10";
            worksheet.Cell(2, 10).Value = "WH1";
            worksheet.Cell(2, 11).Value = "01/03/2025";

            // Save workbook to memory stream.
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            IFormFile file = new FormFile(stream, 0, stream.Length, "excel", "test.xlsx");

            // Set up repository mock: ensure ORDER2 doesn't exist in the database.
            var mockRepo = new Mock<IOrderRepository>();
            mockRepo.Setup(repo => repo.GetByOrderNumberAsync("ORDER2"))
                    .ReturnsAsync((Order)null);
            mockRepo.Setup(repo => repo.AddOrdersAsync(It.IsAny<IEnumerable<Order>>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var orderService = new OrderService(mockRepo.Object);

            // Act
            await orderService.AddOrderAsync(file);

            // Assert: Verify repository.AddOrdersAsync was called exactly once.
            mockRepo.Verify(repo => repo.AddOrdersAsync(It.IsAny<IEnumerable<Order>>()), Times.Once);
        }

    }
}