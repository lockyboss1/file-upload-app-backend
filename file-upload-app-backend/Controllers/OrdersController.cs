using Application.DTOs;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
    {
        await _orderService.AddOrderAsync(orderDto);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderDto.OrderNumber }, orderDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderDto orderDto)
    {
        await _orderService.UpdateOrderAsync(id, orderDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }
}
