using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace file_upload_app_backend.Controllers;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
    {
        var orders = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
        return Ok(orders);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadOrders(IFormFile file)
    {
        try
        {
            await _orderService.AddOrderAsync(file);
            return Ok("Orders processed successfully.");
        }
        catch (CustomValidationException exception)
        {
            return BadRequest(new { errors = exception.Errors });
        }
        catch (Exception ex)
        {
            // For any other exceptions, return a generic error message
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
