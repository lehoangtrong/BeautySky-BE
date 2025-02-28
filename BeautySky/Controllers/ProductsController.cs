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


        // GET: api/Products
        //[HttpGet]
        ////[Authorize(Roles = "Manager, Staff")]
        //public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        //{
        //    var products = await _context.Products.Include(p => p.ProductsImages).ToListAsync();

        //    foreach (var product in products)
        //    {
        //        foreach (var image in product.ProductsImages)
        //        {
        //            image.ImageUrl = image.ImageUrl;
        //        }
        //    }

        //    return Ok(products);
        //}

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductsImages) 
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            foreach (var image in product.ProductsImages)
            {
                image.ImageUrl = image.ImageUrl;
            }

            return Ok(product);
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("UpdateProductById/{id}")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] Product updatedProduct)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updatedProduct.ProductName))
                existingProduct.ProductName = updatedProduct.ProductName;

            if (updatedProduct.Price > 0)
                existingProduct.Price = updatedProduct.Price;

            if (updatedProduct.Quantity >= 0)
                existingProduct.Quantity = updatedProduct.Quantity;

            if (!string.IsNullOrEmpty(updatedProduct.Description))
                existingProduct.Description = updatedProduct.Description;

            if (!string.IsNullOrEmpty(updatedProduct.Ingredient))
                existingProduct.Ingredient = updatedProduct.Ingredient;

            if (updatedProduct.CategoryId > 0)
                existingProduct.CategoryId = updatedProduct.CategoryId;

            if (updatedProduct.SkinTypeId > 0)
                existingProduct.SkinTypeId = updatedProduct.SkinTypeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the product.");
            }
            catch (Exception ex)
            {
                // Log the exception (important!)
                Console.WriteLine($"Error updating product: {ex}");
                return StatusCode(500, "An unexpected error occurred.");
            }

            return Ok();
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