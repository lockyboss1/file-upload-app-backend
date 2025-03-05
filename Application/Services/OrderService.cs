using System.Data;
using System.Globalization;
using Application.DTOs;
using Application.Exceptions;
using Application.Helpers;
using Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using Domain.Interfaces;
using ExcelDataReader;
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

            // Determine file type by extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool isExcelFile = extension == ".xls" || extension == ".xlsx";
            List<OrderDto> records;

            if (extension == ".csv")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    PrepareHeaderForMatch = args => args.Header?.Trim().Replace("*", ""), // Remove asterisks,
                    HeaderValidated = null,
                    MissingFieldFound = null
                });
                // Read CSV records into a list
                records = csv.GetRecords<OrderDto>().ToList();
            }
            else if (extension == ".xls" || extension == ".xlsx")
            {
                // ExcelDataReader requires registering code page provider
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = file.OpenReadStream();
                using var excelReader = ExcelReaderFactory.CreateReader(stream);
                // Configure to use first row as header
                var config = new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                };
                var dataSet = excelReader.AsDataSet(config);
                var dataTable = dataSet.Tables[0];

                // Trim all column names and remove asterisks.
                foreach (DataColumn column in dataTable.Columns)
                {
                    column.ColumnName = column.ColumnName.Trim().Replace("*", "");
                }

                // Convert DataTable rows into a list of OrderDto
                records = new List<OrderDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var orderDto = new OrderDto
                    {
                        OrderNumber = dataTable.Columns.Contains("OrderNumber") ? row["OrderNumber"]?.ToString() : null,
                        AlternateOrderNumber = dataTable.Columns.Contains("AlternateOrderNumber") ? row["AlternateOrderNumber"]?.ToString() : null,
                        OrderDate = dataTable.Columns.Contains("OrderDate") ? row["OrderDate"]?.ToString() : null,
                        ShipToName = dataTable.Columns.Contains("ShipToName") ? row["ShipToName"]?.ToString() : null,
                        ShipToCompany = dataTable.Columns.Contains("ShipToCompany") ? row["ShipToCompany"]?.ToString() : null,
                        ShipToAddress1 = dataTable.Columns.Contains("ShipToAddress1") ? row["ShipToAddress1"]?.ToString() : null,
                        ShipToAddress2 = dataTable.Columns.Contains("ShipToAddress2") ? row["ShipToAddress2"]?.ToString() : null,
                        ShipToAddress3 = dataTable.Columns.Contains("ShipToAddress3") ? row["ShipToAddress3"]?.ToString() : null,
                        ShipToCity = dataTable.Columns.Contains("ShipToCity") ? row["ShipToCity"]?.ToString() : null,
                        ShipToState = dataTable.Columns.Contains("ShipToState") ? row["ShipToState"]?.ToString() : null,
                        ShipToPostalCode = dataTable.Columns.Contains("ShipToPostalCode") ? row["ShipToPostalCode"]?.ToString() : null,
                        ShipToCountry = dataTable.Columns.Contains("ShipToCountry") ? row["ShipToCountry"]?.ToString() : null,
                        ShipToPhone = dataTable.Columns.Contains("ShipToPhone") ? row["ShipToPhone"]?.ToString() : null,
                        ShipToEmail = dataTable.Columns.Contains("ShipToEmail") ? row["ShipToEmail"]?.ToString() : null,
                        Sku = dataTable.Columns.Contains("Sku") ? row["Sku"]?.ToString() : null,
                        Quantity = dataTable.Columns.Contains("Quantity") ? row["Quantity"]?.ToString() : null,
                        RequestedWarehouse = dataTable.Columns.Contains("RequestedWarehouse") ? row["RequestedWarehouse"]?.ToString() : null,
                        DeliveryInstructions = dataTable.Columns.Contains("DeliveryInstructions") ? row["DeliveryInstructions"]?.ToString() : null,
                        Tags = dataTable.Columns.Contains("Tags") ? row["Tags"]?.ToString() : null
                    };

                    records.Add(orderDto);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported file type. Only CSV and Excel files are supported.");
            }

            // Process and validate records
            var orders = new List<Order>();
            var recordErrors = new List<OrderValidationError>();

            foreach (var record in records)
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
                    // Get the raw date string and trim it.
                    string orderDateStr = record.OrderDate.Trim();

                    // If processing an Excel file, remove any time part.
                    if (isExcelFile && orderDateStr.Contains(" "))
                    {
                        orderDateStr = orderDateStr.Split(' ')[0];
                    }

                    // Define allowed date formats.
                    string[] allowedFormats = { "M/d/yyyy", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy" };

                    // Try to parse the date using the allowed formats.
                    if (!DateTime.TryParseExact(orderDateStr, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedOrderDate))
                    {
                        errorsForRecord.Add("OrderDate must be in a valid format (e.g., MM/dd/yyyy or M/d/yyyy).");
                    }
                }

                // Quantity validation: must be a positive integer (disallow non-numeric strings)
                if (!int.TryParse(record.Quantity, out int quantity) || quantity <= 0)
                    errorsForRecord.Add("Quantity must be a positive integer.");

                // Postal Code validation: must be an integer and exactly 5 digits long
                if (!int.TryParse(record.ShipToPostalCode, out _) || record.ShipToPostalCode.Length != 5)
                    errorsForRecord.Add("ShipToPostalCode must be a 5-digit integer.");

                // ShipToState validation: must be exactly two characters
                if (record.ShipToState?.Length != 2)
                    errorsForRecord.Add("ShipToState must be a two-character state abbreviation (e.g., PA).");

                // Accumulate structured errors if any validations failed for this record.
                if (errorsForRecord.Any())
                {
                    recordErrors.Add(new OrderValidationError
                    {
                        OrderNumber = record.OrderNumber,
                        Messages = errorsForRecord
                    });
                }
                else
                {
                    // All validations pass – add the order.
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
                        Quantity = quantity,
                        RequestedWarehouse = record.RequestedWarehouse,
                        DeliveryInstructions = record.DeliveryInstructions,
                        Tags = record.Tags
                    });
                }
            }

            // If there are any validation errors, throw a custom exception and do not save anything.
            if (recordErrors.Any())
            {
                throw new CustomValidationException(recordErrors);
            }

            // Save valid orders to the database if there are no validation errors.
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

