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
using Newtonsoft.Json;
using BeautySky.DTO;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProjectSwpContext _context;
        private readonly IAmazonS3 _amazonS3;
        private readonly string _bucketName = "beautysky";

        public ProductsController(ProjectSwpContext context, IAmazonS3 amazonS3)
        {
            _context = context;
            _amazonS3 = amazonS3;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            int? id = null,
            string? sortBy = null,
            string? order = null,
            string? name = null)
        {
            IQueryable<Product> products = _context.Products.Include(p => p.ProductsImages);

            if (id.HasValue)
            {
                var product = await _context.Products.Include(p => p.ProductsImages).FirstOrDefaultAsync(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }
                return Ok(product);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                products = products.Where(p => p.ProductName.Contains(name));
                if (!await products.AnyAsync())
                {
                    return NotFound("No products found matching the search criteria.");
                }
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "productname":
                        products = (order?.ToLower() == "desc") ? products.OrderByDescending(p => p.ProductName) : products.OrderBy(p => p.ProductName);
                        break;
                    case "price":
                        products = (order?.ToLower() == "desc") ? products.OrderByDescending(p => p.Price) : products.OrderBy(p => p.Price);
                        break;
                    default:
                        return BadRequest("Invalid sortBy parameter. Use 'ProductName' or 'Price'.");
                }
            }

            return await products.ToListAsync();
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductDTO productDto)
        {
            try
            {
                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    Price = productDto.Price ?? 0,
                    Quantity = productDto.Quantity ?? 0,
                    Description = productDto.Description,
                    Ingredient = productDto.Ingredient,
                    CategoryId = productDto.CategoryId,
                    SkinTypeId = productDto.SkinTypeId
                };

                if (productDto.File != null && productDto.File.Length > 0)
                {
                    string keyName = $"products/{Guid.NewGuid()}_{productDto.File.FileName}";
                    using (var stream = productDto.File.OpenReadStream())
                    {
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = keyName,
                            InputStream = stream,
                            ContentType = productDto.File.ContentType
                        };
                        await _amazonS3.PutObjectAsync(putRequest);
                    }

                    string fileUrl = $"https://beautysky.s3.amazonaws.com/{keyName}";
                    product.ProductsImages = new List<ProductsImage>
            {
                new ProductsImage { ImageUrl = fileUrl, ImageDescription = productDto.ImageDescription }
            };
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducts), new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex}");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDTO productDto)
        {
            try
            {
                var product = await _context.Products.Include(p => p.ProductsImages).FirstOrDefaultAsync(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                product.ProductName = productDto.ProductName ?? product.ProductName;
                product.Price = productDto.Price ?? product.Price;
                product.Quantity = productDto.Quantity ?? product.Quantity;
                product.Description = productDto.Description ?? product.Description;
                product.Ingredient = productDto.Ingredient ?? product.Ingredient;
                product.CategoryId = productDto.CategoryId ?? product.CategoryId;
                product.SkinTypeId = productDto.SkinTypeId ?? product.SkinTypeId;

                if (productDto.File != null && productDto.File.Length > 0)
                {
                    var existingImage = product.ProductsImages.FirstOrDefault();
                    if (existingImage != null)
                    {
                        // Xóa ảnh cũ trên S3
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = existingImage.ImageUrl.Replace("https://beautysky.s3.amazonaws.com/", "")
                        };
                        await _amazonS3.DeleteObjectAsync(deleteRequest);

                        string keyName = $"products/{Guid.NewGuid()}_{productDto.File.FileName}";
                        using (var stream = productDto.File.OpenReadStream())
                        {
                            var putRequest = new PutObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = keyName,
                                InputStream = stream,
                                ContentType = productDto.File.ContentType
                            };
                            await _amazonS3.PutObjectAsync(putRequest);
                        }

                        string fileUrl = $"https://beautysky.s3.amazonaws.com/{keyName}";
                        existingImage.ImageUrl = fileUrl;
                        existingImage.ImageDescription = productDto.ImageDescription ?? existingImage.ImageDescription;
                    }
                    else
                    {
                        string keyName = $"products/{Guid.NewGuid()}_{productDto.File.FileName}";
                        using (var stream = productDto.File.OpenReadStream())
                        {
                            var putRequest = new PutObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = keyName,
                                InputStream = stream,
                                ContentType = productDto.File.ContentType
                            };
                            await _amazonS3.PutObjectAsync(putRequest);
                        }

                        string fileUrl = $"https://beautysky.s3.amazonaws.com/{keyName}";
                        product.ProductsImages.Add(new ProductsImage
                        {
                            ImageUrl = fileUrl,
                            ImageDescription = productDto.ImageDescription
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex}");
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.Include(p => p.ProductsImages).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            foreach (var image in product.ProductsImages)
            {
                var deleteRequest = new DeleteObjectRequest { BucketName = _bucketName, Key = $"products/{Path.GetFileName(image.ImageUrl)}" };
                await _amazonS3.DeleteObjectAsync(deleteRequest);
                _context.ProductsImages.Remove(image);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok("Product deleted successfully");
        }
    }
}
