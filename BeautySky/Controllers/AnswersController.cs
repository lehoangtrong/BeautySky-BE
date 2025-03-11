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
    public class AnswersController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public AnswersController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/Answers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswers()
        {
            return await _context.Answers.ToListAsync();
        }

        // GET: api/Answers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Answer>> GetAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);

            if (answer == null)
            {
                return NotFound("Answer not found");
            }

            return answer;
        }

        // PUT: api/Answers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnswer(int id, [FromBody] Answer updateAnswer)
        {
            var existingAnswer = await _context.Answers.FindAsync(id);
            if (existingAnswer == null)
            {
                return NotFound("Answer not found");
            }

            if (!string.IsNullOrEmpty(updateAnswer.AnswerText))
                existingAnswer.AnswerText = updateAnswer.AnswerText;
            if (!string.IsNullOrEmpty(updateAnswer.SkinTypeId))
                existingAnswer.SkinTypeId = updateAnswer.SkinTypeId;
            if (!string.IsNullOrEmpty(updateAnswer.Point))
                existingAnswer.Point = updateAnswer.Point;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the Answer.");
            }
            return Ok("Update Successful");
        }

        // POST: api/Answers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Answer>> PostAnswer(Answer answer)
        {
            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnswer", new { id = answer.AnswerId }, answer);
        }

        // DELETE: api/Answers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
            {
                return NotFound("Answer not found");
            }

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return Ok("Delete success");
        }

        private bool AnswerExists(int id)
        {
            return _context.Answers.Any(e => e.AnswerId == id);
        }
    }
}
