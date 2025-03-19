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
    public class CarePlanStepsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public CarePlanStepsController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/CarePlanSteps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarePlanStep>>> GetCarePlanSteps()
        {
            return await _context.CarePlanSteps.ToListAsync();
        }

        // GET: api/CarePlanSteps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarePlanStep>> GetCarePlanStep(int id)
        {
            var carePlanStep = await _context.CarePlanSteps.FindAsync(id);

            if (carePlanStep == null)
            {
                return NotFound();
            }

            return carePlanStep;
        }

        // PUT: api/CarePlanSteps/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarePlanStep(int id, [FromBody] CarePlanStep updateCarePlanStep)
        {
            var existingCarePlanStep = await _context.CarePlanSteps.FindAsync(id);
            if (existingCarePlanStep == null)
            {
                return NotFound("Care Plan Step not found");
            }
            if (!string.IsNullOrEmpty(updateCarePlanStep.StepName))
                existingCarePlanStep.StepName = updateCarePlanStep.StepName;
            if (!string.IsNullOrEmpty(updateCarePlanStep.StepDescription))
                existingCarePlanStep.StepDescription = updateCarePlanStep.StepDescription;
            //if (updateCarePlanStep.StepOrder > 0)
            //    existingCarePlanStep.StepOrder = updateCarePlanStep.StepOrder;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the care plan step.");
            }
            return Ok("Update Successful");
        }

        // POST: api/CarePlanSteps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CarePlanStep>> PostCarePlanStep(CarePlanStep carePlanStep)
        {
            _context.CarePlanSteps.Add(carePlanStep);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCarePlanStep", new { id = carePlanStep.StepId }, carePlanStep);
        }

        // DELETE: api/CarePlanSteps/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarePlanStep(int id)
        {
            var carePlanStep = await _context.CarePlanSteps.FindAsync(id);
            if (carePlanStep == null)
            {
                return NotFound();
            }

            _context.CarePlanSteps.Remove(carePlanStep);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CarePlanStepExists(int id)
        {
            return _context.CarePlanSteps.Any(e => e.StepId == id);
        }
    }
}
