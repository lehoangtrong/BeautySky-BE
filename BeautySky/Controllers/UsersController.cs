using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.DTO;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public UsersController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet("Get All User")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("Get User By ID")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!string.IsNullOrEmpty(updatedUser.UserName))
                existingUser.UserName = updatedUser.UserName;
            if (!string.IsNullOrEmpty(updatedUser.FullName))
                existingUser.FullName = updatedUser.FullName;

            if (!string.IsNullOrEmpty(updatedUser.Email))
                existingUser.Email = updatedUser.Email;

            if (!string.IsNullOrEmpty(updatedUser.Password))
                existingUser.Password = updatedUser.Password;

            if (!string.IsNullOrEmpty(updatedUser.ConfirmPassword))
                existingUser.ConfirmPassword = updatedUser.ConfirmPassword;

            if (updatedUser.RoleId.HasValue)
                existingUser.RoleId = updatedUser.RoleId;

            if (!string.IsNullOrEmpty(updatedUser.Phone))
                existingUser.Phone = updatedUser.Phone;

            if (!string.IsNullOrEmpty(updatedUser.Address))
                existingUser.Address = updatedUser.Address;

            if (updatedUser.IsActive.HasValue)
                existingUser.IsActive = updatedUser.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Update Successful", user = existingUser });
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(User user)
        {

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email || u.UserName == user.UserName);

            if (existingUser != null)
            {
                return BadRequest("Email hoặc Username đã được sử dụng.");
            }
            if (user.Password != user.ConfirmPassword)
            {
                return BadRequest("Mật khẩu và xác nhận mật khẩu không khớp.");
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Login Success");
        }

        // DELETE: api/Users/5
        [HttpDelete("Delete User By ID")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            //user.IsActive = false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
