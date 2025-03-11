using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarePlansController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public CarePlansController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/CarePlans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarePlan>>> GetCarePlans()
        {
            return await _context.CarePlans.ToListAsync();
        }

        // GET: api/CarePlans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarePlan>> GetCarePlan(int id)
        {
            var carePlan = await _context.CarePlans.FindAsync(id);

            if (carePlan == null)
            {
                return NotFound();
            }

            return carePlan;
        }

        // PUT: api/CarePlans/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarePlan(int id, CarePlan carePlan)
        {
            if (id != carePlan.CarePlanId)
            {
                return BadRequest();
            }

            _context.Entry(carePlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarePlanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CarePlans
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CarePlan>> PostCarePlan(CarePlan carePlan)
        {
            _context.CarePlans.Add(carePlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCarePlan", new { id = carePlan.CarePlanId }, carePlan);
        }

        // DELETE: api/CarePlans/5
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

        private bool CarePlanExists(int id)
        {
            return _context.CarePlans.Any(e => e.CarePlanId == id);
        }
    }
}
