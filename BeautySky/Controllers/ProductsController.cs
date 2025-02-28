using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public ProductsController(ProjectSwpContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
    int? id = null,
    string? sortBy = null,
    string? order = null,
    string? name = null)
        {
            IQueryable<Product> products = _context.Products.Include(p => p.ProductsImages);

            // Lấy theo ID
            if (id.HasValue)
            {
                var product = await _context.Products.Include(p => p.ProductsImages).FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                return Ok(new List<Product> { product }); // Trả về một danh sách chứa sản phẩm
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrWhiteSpace(name))
            {
                products = products.Where(p => p.ProductName.Contains(name));
                if (!await products.AnyAsync())
                {
                    return NotFound("No products found matching the search criteria.");
                }
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "productname":
                        products = (order?.ToLower() == "desc")
                            ? products.OrderByDescending(p => p.ProductName)
                            : products.OrderBy(p => p.ProductName);
                        break;
                    case "price":
                        products = (order?.ToLower() == "desc")
                            ? products.OrderByDescending(p => p.Price)
                            : products.OrderBy(p => p.Price);
                        break;
                    default:
                        return BadRequest("Invalid sortBy parameter. Use 'ProductName' or 'Price'.");
                }
            }

            return await products.ToListAsync();
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest("ID in the request body does not match the ID in the route.");
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound("Product not found.");
                }
                else
                {
                    return StatusCode(500, "An error occurred while updating the product.  Please try again."); // More informative error.
                }
            }
            catch (Exception ex)
            {
                // Log the exception (important!)
                Console.WriteLine($"Error updating product: {ex}");
                return StatusCode(500, "An unexpected error occurred.");
            }

            return Ok(new { message = "Product updated successfully." });
        }


        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducts), new { id = product.ProductId }, new { message = "Product created successfully.", product });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating product: {ex}");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }


        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting product: {ex}");
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}