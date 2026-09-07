using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantShopAPI.Models;

namespace PlantShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly PlantStoreDbContext _context;

        public OrderController(PlantStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            return Ok(order);
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, [FromQuery] int userId = 1)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Đơn hàng phải có ít nhất 1 sản phẩm.");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var plant = await _context.Plants.FindAsync(item.PlantId);
                if (plant == null)
                    return BadRequest($"Không tìm thấy cây trồng với ID = {item.PlantId}");

                var unitPrice = plant.Price;
                totalAmount += unitPrice * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.PlantId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });
            }

            totalAmount = Math.Max(0, totalAmount - dto.Discount);

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = "Đang xử lý",
                OrderDate = DateTime.Now,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
        }

        // PUT: api/Order/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa đơn hàng thành công." });
        }
    }
}