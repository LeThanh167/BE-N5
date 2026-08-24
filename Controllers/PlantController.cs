using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantShopAPI.Models;

namespace PlantShopAPI.Controllers
{
    [Route("api/plants")]
    [ApiController]
    public class PlantController : ControllerBase
    {
        // 1. Lấy danh sách cây
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = DataStore.Plants.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = query.Count();
            var result = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { totalItems, page, pageSize, data = result });
        }

        // 2. Xem chi tiết
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var plant = DataStore.Plants.FirstOrDefault(p => p.Id == id);
            if (plant == null) return NotFound(new { message = "Không tìm thấy cây cảnh!" });
            return Ok(plant);
        }

        // 3. Thêm cây mới
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public IActionResult Create([FromBody] Plant newPlant)
        {
            newPlant.Id = DataStore.Plants.Max(p => p.Id) + 1;
            DataStore.Plants.Add(newPlant);
            return CreatedAtAction(nameof(GetById), new { id = newPlant.Id }, newPlant);
        }

        // 4. Cập nhật cây
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Plant updatedPlant)
        {
            var plant = DataStore.Plants.FirstOrDefault(p => p.Id == id);
            if (plant == null) return NotFound(new { message = "Không tìm thấy cây cảnh!" });

            plant.Name = updatedPlant.Name;
            plant.Price = updatedPlant.Price;
            plant.Quantity = updatedPlant.Quantity;
            plant.Description = updatedPlant.Description;

            return Ok(new { message = "Cập nhật thành công!", plant });
        }

        // 5. Xóa cây
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var plant = DataStore.Plants.FirstOrDefault(p => p.Id == id);
            if (plant == null) return NotFound(new { message = "Không tìm thấy cây cảnh!" });

            DataStore.Plants.Remove(plant);
            return Ok(new { message = "Xóa cây thành công!" });
        }
    }
}