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
    public class QuestionsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public QuestionsController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions.ToListAsync();
        }

        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound("Quiz not found");
            }

            return question;
        }

        // PUT: api/Questions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(int id, [FromBody] Question updatedQuestion)
        {
            var existingQuestion = await _context.Questions.FindAsync(id);
            if (existingQuestion == null)
            {
                return NotFound("Quiz not found");
            }

            if (updatedQuestion.QuestionId > 0)
                existingQuestion.QuestionId = updatedQuestion.QuestionId;

            if (!string.IsNullOrEmpty(updatedQuestion.QuestionText))
                existingQuestion.QuestionText = updatedQuestion.QuestionText;

            if (updatedQuestion.OrderNumber > 0)
                existingQuestion.OrderNumber = updatedQuestion.OrderNumber;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the Question.");
            }

            return Ok("Update Successful");
        }

        // POST: api/Questions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestion(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok("Add quiz success");
        }

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound("Quiz not found");
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok("Delete success");
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}
