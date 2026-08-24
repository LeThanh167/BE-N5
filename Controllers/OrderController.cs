using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantShopAPI.Models;
using System.Security.Claims;

namespace PlantShopAPI.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        // 1. NGHIỆP VỤ ĐẶT HÀNG (POST /api/orders)
        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreateOrderDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst(ClaimTypes.Email)?.Value 
                         ?? "UnknownUser";

            if (request.Items == null || !request.Items.Any())
                return BadRequest(new { message = "Giỏ hàng không được để trống!" });

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var plant = DataStore.Plants.FirstOrDefault(p => p.Id == item.PlantId);
                if (plant == null)
                    return NotFound(new { message = $"Không tìm thấy cây cảnh có ID: {item.PlantId}" });

                if (plant.Quantity < item.Quantity)
                    return BadRequest(new { message = $"Cây '{plant.Name}' chỉ còn {plant.Quantity} sản phẩm trong kho!" });

                decimal itemTotal = plant.Price * item.Quantity;
                totalAmount += itemTotal;

                orderItems.Add(new OrderItem
                {
                    PlantId = plant.Id,
                    Quantity = item.Quantity,
                    Price = plant.Price // Lưu lại giá tại thời điểm mua
                });

                // Trừ tồn kho
                plant.Quantity -= item.Quantity;
            }

            decimal finalAmount = totalAmount - request.Discount;
            if (finalAmount < 0) finalAmount = 0;

            var newOrder = new Order
            {
                Id = DataStore.Orders.Count + 1,
                UserId = userId,
                TotalAmount = totalAmount,
                Discount = request.Discount,
                FinalAmount = finalAmount,
                Status = "PENDING",
                CreatedAt = DateTime.Now,
                OrderItems = orderItems
            };

            DataStore.Orders.Add(newOrder);

            return Ok(new { message = "Đặt hàng thành công!", order = newOrder });
        }

        // 2. XEM DANH SÁCH ĐƠN HÀNG (GET /api/orders)
        [HttpGet]
        public IActionResult GetOrders()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst(ClaimTypes.Email)?.Value;

            if (role == "ADMIN")
            {
                return Ok(DataStore.Orders);
            }

            var userOrders = DataStore.Orders.Where(o => o.UserId == userId).ToList();
            return Ok(userOrders);
        }

        // 3. XEM CHI TIẾT 1 ĐƠN HÀNG (GET /api/orders/{id}) - BỔ SUNG CHUẨN KẾ HOẠCH
        [HttpGet("{id}")]
        public IActionResult GetOrderById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            if (role != "ADMIN" && order.UserId != userId)
                return Forbid();

            return Ok(order);
        }

        // 4. NGHIỆP VỤ HỦY ĐƠN HÀNG (PUT /api/orders/{id}/cancel)
        [HttpPut("{id}/cancel")]
        public IActionResult CancelOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            if (role != "ADMIN" && order.UserId != userId)
                return Forbid();

            if (order.Status == "CANCELLED")
                return BadRequest(new { message = "Đơn hàng này đã bị hủy trước đó!" });

            // Kiểm tra trạng thái đơn: Chỉ cho phép hủy khi PENDING hoặc CONFIRMED
            if (order.Status != "PENDING" && order.Status != "CONFIRMED")
            {
                return BadRequest(new { 
                    message = "Không thể hủy đơn hàng khi đã chuyển sang trạng thái đang giao (SHIPPING) hoặc đã hoàn thành (COMPLETED)!" 
                });
            }

            order.Status = "CANCELLED";

            // Hoàn lại tồn kho
            foreach (var item in order.OrderItems)
            {
                var plant = DataStore.Plants.FirstOrDefault(p => p.Id == item.PlantId);
                if (plant != null)
                {
                    plant.Quantity += item.Quantity;
                }
            }

            return Ok(new { message = "Hủy đơn hàng thành công, đã hoàn lại tồn kho!", order });
        }

        // 5. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (PUT /api/orders/{id}/status) - BỔ SUNG CHUẨN KẾ HOẠCH
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusDto request)
        {
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            string newStatus = request.Status.ToUpper();
            var validStatuses = new[] { "PENDING", "CONFIRMED", "SHIPPING", "COMPLETED", "CANCELLED" };

            if (!validStatuses.Contains(newStatus))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ! Các trạng thái hợp lệ: PENDING, CONFIRMED, SHIPPING, COMPLETED, CANCELLED" });
            }

            order.Status = newStatus;
            return Ok(new { message = "Cập nhật trạng thái đơn hàng thành công!", order });
        }
    }
}