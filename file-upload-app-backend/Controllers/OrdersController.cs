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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadOrders(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        await _orderService.AddOrderAsync(file);
        return Ok("File processed successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }
}
