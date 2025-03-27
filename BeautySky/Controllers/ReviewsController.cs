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
    public class ReviewsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public ReviewsController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReviews()
        {
            var reviews = await _context.Reviews
                .Where(r => r.IsActive != false) // Chỉ lấy các review chưa bị ẩn
                .Include(r => r.Product)
                .Include(r => r.User)
                .Select(r => new {
                    r.ReviewId,
                    r.ProductId,
                    ProductName = r.Product != null ? r.Product.ProductName : "Không xác định",
                    r.UserId,
                    UserName = r.User != null ? r.User.FullName : "Không xác định",
                    r.Rating,
                    r.Comment,
                    r.ReviewDate,
                    r.IsActive // Thêm trường IsActive vào response
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound("Review not found");
            }

            return review;
        }

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            try
            {
                // Set IsActive = true cho review mới
                review.IsActive = true;
                review.ReviewDate = DateTime.Now; // Thêm ngày tạo review

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Trả về thông tin chi tiết của review vừa tạo
                var newReview = await _context.Reviews
                    .Include(r => r.Product)
                    .Include(r => r.User)
                    .Where(r => r.ReviewId == review.ReviewId)
                    .Select(r => new {
                        r.ReviewId,
                        r.ProductId,
                        ProductName = r.Product != null ? r.Product.ProductName : "Không xác định",
                        r.UserId,
                        UserName = r.User != null ? r.User.FullName : "Không xác định",
                        r.Rating,
                        r.Comment,
                        r.ReviewDate,
                        r.IsActive
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    message = "Review created successfully",
                    data = newReview
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the review",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound("Review not found");
            }

            review.IsActive = false;
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return Ok("Delete success");
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
