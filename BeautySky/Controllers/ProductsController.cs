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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("Get Product By ID")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {   
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/Sort
        [HttpGet("Sort")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<IEnumerable<Product>>> GetSortedProducts(
            [FromQuery] string sortBy = "ProductName",
            [FromQuery] string order = "asc")
        {
            IQueryable<Product> products = _context.Products;

            // Xử lý sắp xếp
            switch (sortBy.ToLower())
            {
                case "productname":
                    products = (order.ToLower() == "desc")
                        ? products.OrderByDescending(p => p.ProductName)
                        : products.OrderBy(p => p.ProductName);
                    break;
                case "price":
                    products = (order.ToLower() == "desc")
                        ? products.OrderByDescending(p => p.Price)
                        : products.OrderBy(p => p.Price);
                    break;
                default:
                    return BadRequest("Invalid sortBy parameter. Use 'ProductName' or 'Price'.");
            }

            return await products.ToListAsync();
        }

        // GET: api/Products/Search
        [HttpGet("Search")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Product name cannot be empty.");
            }

            var products = await _context.Products
                .Where(p => p.ProductName.Contains(name))
                .ToListAsync();

            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            return products;
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Product")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Product")]
        //[Authorize(Roles = "Manager, Staff")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("Product")]
        //[Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
