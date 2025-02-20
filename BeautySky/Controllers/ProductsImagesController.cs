using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using Amazon.S3.Model;
using Amazon.S3;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsImagesController : ControllerBase
    {
        private readonly ProjectSwpContext _context;
        private readonly IAmazonS3 _amazonS3;
        private readonly string _bucketName = "beautysky";

        public ProductsImagesController(ProjectSwpContext context, IAmazonS3 amazonS3)
        {
            _context = context;
            _amazonS3 = amazonS3;
        }

        // GET: api/ProductsImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsImage>>> GetProductsImages()
        {
            return await _context.ProductsImages.ToListAsync();
        }

        // GET: api/ProductsImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductsImage>> GetProductsImage(int id)
        {
            var productsImage = await _context.ProductsImages.FindAsync(id);

            if (productsImage == null)
            {
                return NotFound();
            }

            return productsImage;
        }

        // PUT: api/ProductsImages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductsImage(int id, ProductsImage productsImage)
        {
            if (id != productsImage.ProductsImageId)
            {
                return BadRequest();
            }

            _context.Entry(productsImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductsImageExists(id))
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

        // POST: api/ProductsImages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost("UploadFile")]

        public async Task<IActionResult> UploadImage(int productId, IFormFile file, string? imageDescription)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file upload");

            try
            {
                string keyName = $"products/{Guid.NewGuid()}_{file.FileName}";

                using (var stream = file.OpenReadStream())
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = keyName,
                        InputStream = stream, 
                        ContentType = file.ContentType,
                        
                    };

                    await _amazonS3.PutObjectAsync(putRequest);
                }



                //Tạo URL truy cập File
                string fileUrl = $"https://beautysky.s3.amazonaws.com/{keyName}";

                //Lưu vào DB
                var productsImage = new ProductsImage
                {
                    ImageUrl = fileUrl,
                    ProductId = productId,
                    ImageDescription = imageDescription
                };

                _context.ProductsImages.Add(productsImage);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Upload successful", imageUrl = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload failed", error = ex.Message });
            }

        }


        //[HttpPost]
        //public async Task<ActionResult<ProductsImage>> PostProductsImage(ProductsImage productsImage)
        //{
        //    _context.ProductsImages.Add(productsImage);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetProductsImage", new { id = productsImage.ProductsImageId }, productsImage);
        //}

        // DELETE: api/ProductsImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductsImage(int id)
        {
            var productsImage = await _context.ProductsImages.FindAsync(id);
            if (productsImage == null)
            {
                return NotFound();
            }

            _context.ProductsImages.Remove(productsImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductsImageExists(int id)
        {
            return _context.ProductsImages.Any(e => e.ProductsImageId == id);
        }
    }
}
