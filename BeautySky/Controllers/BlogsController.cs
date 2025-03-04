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
    [Produces("application/json")]
    public class BlogsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public BlogsController(ProjectSwpContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all blogs.
        /// </summary>
        /// <returns>List of blogs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
        {
            return await _context.Blogs.ToListAsync();
        }

        /// <summary>
        /// Get a specific blog by ID.
        /// </summary>
        /// <param name="id">Blog ID</param>
        /// <returns>A blog</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        /// <summary>
        /// Update a blog.
        /// </summary>
        /// <param name="id">Blog ID</param>
        /// <param name="blog">Blog object</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBlog(int id, Blog blog)
        {
            if (id != blog.BlogId)
            {
                return BadRequest();
            }

            _context.Entry(blog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
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

        /// <summary>
        /// Create a new blog.
        /// </summary>
        /// <param name="blog">Blog object</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Thêm response type cho BadRequest
        public async Task<ActionResult<object>> PostBlog(DTO.BlogCreateRequest blogCreateRequest)
        {
            // 1. Tạo đối tượng Blog mới từ dữ liệu nhận được
            Blog blog = new Blog
            {
                Title = blogCreateRequest.Title,
                Content = blogCreateRequest.Content,
                AuthorId = blogCreateRequest.AuthorId,
                Status = blogCreateRequest.Status,
                SkinType = blogCreateRequest.SkinType,
                Category = blogCreateRequest.Category,
                ImgURL = blogCreateRequest.ImgURL,

                // 2. Các trường tự động tạo
                CreatedDate = DateTime.UtcNow, // Sử dụng UtcNow để đảm bảo tính nhất quán
                UpdatedDate = DateTime.UtcNow,
            };

            // 3. Thêm blog vào database
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            // 4. Tạo đối tượng response chỉ chứa các trường mong muốn
            var response = new
            {
                Title = blog.Title,
                Content = blog.Content,
                AuthorId = blog.AuthorId,
                SkinType = blog.SkinType,
                Category = blog.Category,
                ImgURL = blog.ImgURL
            };

            // 5. Trả về response với status code 201 (Created)
            return CreatedAtAction("GetBlog", new { id = blog.BlogId }, response);
        }

        /// <summary>
        /// Delete a blog by ID.
        /// </summary>
        /// <param name="id">Blog ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }

        [HttpGet("by-skin-type/{skinType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogsBySkinType(string skinType)
        {
            var blogs = await _context.Blogs
                .Where(b => b.SkinType == skinType)
                .ToListAsync();

            return Ok(blogs);
        }

        [HttpGet("by-category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogsByCategory(string category)
        {
            var blogs = await _context.Blogs
                .Where(b => b.Category == category)
                .ToListAsync();

            return Ok(blogs);
        }
    }
}