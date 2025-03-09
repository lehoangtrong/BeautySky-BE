using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager, Staff")]
    public class CarePlanController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public CarePlanController(ProjectSwpContext context)
        {
            _context = context;
        }

        // Lấy tất cả lộ trình
        [HttpGet]
        public async Task<IActionResult> GetCarePlans()
        {
            var carePlans = await _context.CarePlans
                .Include(cp => cp.CarePlanStep)
                .ToListAsync();
            return Ok(carePlans);
        }

        // Lấy lộ trình theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarePlan(int id)
        {
            var carePlan = await _context.CarePlans
                .Include(cp => cp.CarePlanStep)
                .FirstOrDefaultAsync(cp => cp.CarePlanId == id);
            if (carePlan == null)
            {
                return NotFound();
            }
            return Ok(carePlan);
        }

        // Thêm lộ trình mới
        [HttpPost]
        public async Task<IActionResult> CreateCarePlan([FromBody] CarePlan carePlan)
        {
            _context.CarePlans.Add(carePlan);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCarePlan), new { id = carePlan.CarePlanId }, carePlan);
        }

        // Cập nhật lộ trình
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCarePlan(int id, [FromBody] CarePlan carePlan)
        {
            if (id != carePlan.CarePlanId)
            {
                return BadRequest();
            }
            _context.Entry(carePlan).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Xóa lộ trình
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarePlan(int id)
        {
            var carePlan = await _context.CarePlans.FindAsync(id);
            if (carePlan == null)
            {
                return NotFound();
            }
            _context.CarePlans.Remove(carePlan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}