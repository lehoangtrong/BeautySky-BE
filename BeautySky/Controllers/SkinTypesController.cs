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
    public class SkinTypesController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public SkinTypesController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/SkinTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkinType>>> GetSkinTypes()
        {
            return await _context.SkinTypes.ToListAsync();
        }

        // GET: api/SkinTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkinType>> GetSkinType(int id)
        {
            var skinType = await _context.SkinTypes.FindAsync(id);

            if (skinType == null)
            {
                return NotFound("SkinType not found");
            }

            return skinType;
        }

        // PUT: api/SkinTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkinType(int id, [FromBody] SkinType updatedSkinType)
        {
            var existingSkinTypes = await _context.SkinTypes.FindAsync(id);
            if (existingSkinTypes == null)
            {
                return NotFound("SkinType not found");
            }

            if (!string.IsNullOrEmpty(updatedSkinType.SkinTypeName))
                existingSkinTypes.SkinTypeName = updatedSkinType.SkinTypeName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the SkinType.");
            }

            return Ok("Update Successful");
        }

        // POST: api/SkinTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SkinType>> PostSkinType(SkinType skinType)
        {
            _context.SkinTypes.Add(skinType);
            await _context.SaveChangesAsync();

            return Ok("Add skinType success");
        }

        // DELETE: api/SkinTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkinType(int id)
        {
            var skinType = await _context.SkinTypes.FindAsync(id);
            if (skinType == null)
            {
                return NotFound("SkinType not found");
            }

            _context.SkinTypes.Remove(skinType);
            await _context.SaveChangesAsync();

            return Ok("Delete success");
        }

        private bool SkinTypeExists(int id)
        {
            return _context.SkinTypes.Any(e => e.SkinTypeId == id);
        }
    }
}
