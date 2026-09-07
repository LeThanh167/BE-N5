using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantShopAPI.Models;

namespace PlantShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly PlantStoreDbContext _context;

        public PlantController(PlantStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Plant
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Plants.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.PlantName.Contains(search));
            }

            var plants = await query.ToListAsync();
            return Ok(plants);
        }

        // GET: api/Plant/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plant = await _context.Plants.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.PlantId == id);

            if (plant == null) return NotFound("Không tìm thấy cây trồng.");

            return Ok(plant);
        }

        // POST: api/Plant
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Plant plant)
        {
            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = plant.PlantId }, plant);
        }

        // PUT: api/Plant/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Plant updatedPlant)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant == null) return NotFound("Không tìm thấy cây trồng.");

            plant.PlantName = updatedPlant.PlantName;
            plant.Price = updatedPlant.Price;
            plant.ImageUrl = updatedPlant.ImageUrl;
            plant.CategoryId = updatedPlant.CategoryId;

            await _context.SaveChangesAsync();
            return Ok(plant);
        }

        // DELETE: api/Plant/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant == null) return NotFound("Không tìm thấy cây trồng.");

            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa cây trồng thành công." });
        }
    }
}