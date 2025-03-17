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
                return NotFound("Image not found");
            }

            return productsImage;
        }

        // PUT: api/ProductsImages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductsImage(int id, IFormFile? file, [FromForm] string? imageDescription)
        {
            var existingImage = await _context.ProductsImages.FindAsync(id);
            if (existingImage == null)
            {
                return NotFound("Image not found");
            }

            // Cập nhật ImageDescription nếu có
            if (!string.IsNullOrEmpty(imageDescription))
            {
                existingImage.ImageDescription = imageDescription;
            }

            // Nếu không có file ảnh mới, chỉ cập nhật ImageDescription
            if (file == null)
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Image description updated successfully", imageUrl = existingImage.ImageUrl, imageDescription = existingImage.ImageDescription });
            }

            // Xóa ảnh cũ trong S3 nếu có
            string oldImageUrl = existingImage.ImageUrl;
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                var oldFileName = Path.GetFileName(oldImageUrl);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = "beautysky",
                    Key = $"products/{oldFileName}"
                };

                var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2);
                await s3Client.DeleteObjectAsync(deleteRequest);
            }

            // Tạo file mới với GUID để tránh trùng tên
            var newFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var newFilePath = $"products/{newFileName}";

            using (var stream = file.OpenReadStream())
            {
                var uploadRequest = new PutObjectRequest
                {
                    BucketName = "beautysky",
                    Key = newFilePath,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2);
                await s3Client.PutObjectAsync(uploadRequest);
            }

            // Cập nhật URL ảnh mới trong database
            existingImage.ImageUrl = $"https://beautysky.s3.amazonaws.com/{newFilePath}";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Concurrency error occurred while updating the image.");
            }

            return Ok(new { message = "Image updated successfully", imageUrl = existingImage.ImageUrl });
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

        // DELETE: api/ProductsImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductsImage(int id)
        {
            var productsImage = await _context.ProductsImages.FindAsync(id);
            if (productsImage == null)
            {
                return NotFound("Image not found");
            }

            // Xóa ảnh trên S3
            if (!string.IsNullOrEmpty(productsImage.ImageUrl))
            {
                var fileName = Path.GetFileName(productsImage.ImageUrl);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"products/{fileName}"
                };

                await _amazonS3.DeleteObjectAsync(deleteRequest);
            }

            _context.ProductsImages.Remove(productsImage);
            await _context.SaveChangesAsync();

            return Ok("Delete success");
        }

        private bool ProductsImageExists(int id)
        {
            return _context.ProductsImages.Any(e => e.ProductsImageId == id);
        }
    }
}
