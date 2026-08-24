using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PlantShopAPI.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // API này ai cũng gọi được (không cần token)
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new { message = "Đây là API công khai, không cần đăng nhập" });
        }

        // API này bắt buộc phải có JWT
        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Bạn đã truy cập API riêng tư thành công!",
                email,
                fullName,
                role
            });
        }

        // API chỉ ADMIN mới gọi được
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Chỉ ADMIN mới thấy được nội dung này" });
        }
    }
}