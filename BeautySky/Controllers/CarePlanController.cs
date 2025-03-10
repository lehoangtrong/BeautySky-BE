using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarePlanController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public CarePlanController(ProjectSwpContext context)
        {
            _context = context;
        }

        // Helper method to get products by skin type and step order
        private List<Product> GetProductsByCriteria(int? skinTypeId, int stepOrder)
        {
            return _context.Products
                .Where(p => p.SkinTypeId == skinTypeId && p.CategoryId == stepOrder)
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetCarePlans()
        {
            var carePlans = await _context.CarePlans
                .Include(cp => cp.CarePlanStep)
                    .ThenInclude(cps => cps.CarePlanProducts)
                        .ThenInclude(cpp => cpp.Product)
                .ToListAsync();

            var carePlanDtos = carePlans.Select(cp => new CarePlanDto
            {
                CarePlanId = cp.CarePlanId,
                PlanName = cp.PlanName,
                Description = cp.Description,
                Steps = cp.CarePlanStep
                    .OrderBy(s => s.StepOrder)
                    .Select(s => new CarePlanStepDto
                    {
                        StepId = s.StepId,
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        StepDescription = s.StepDescription,
                        Products = s.CarePlanProducts
                            .Select(cpp => new ProductDto
                            {
                                ProductId = cpp.ProductId,
                                ProductName = cpp.Product.ProductName,
                                Description = cpp.Product.Description
                            }).ToList()
                    }).ToList()
            }).ToList();

            return Ok(carePlanDtos);
        }

        // Lấy lộ trình theo ID với các bước và sản phẩm
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarePlan(int id)
        {
            var carePlan = await _context.CarePlans
                .Include(cp => cp.CarePlanStep)
                    .ThenInclude(cps => cps.CarePlanProducts)
                        .ThenInclude(cpp => cpp.Product) // Include Product data
                .FirstOrDefaultAsync(cp => cp.CarePlanId == id);

            if (carePlan == null)
            {
                return NotFound();
            }

            var carePlanDto = new CarePlanDto
            {
                CarePlanId = carePlan.CarePlanId,
                PlanName = carePlan.PlanName,
                Description = carePlan.Description,
                Steps = carePlan.CarePlanStep
                    .OrderBy(s => s.StepOrder)
                    .Select(s => new CarePlanStepDto
                    {
                        StepId = s.StepId,
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        StepDescription = s.StepDescription,
                        Products = carePlan.CarePlanStep.FirstOrDefault(cs => cs.StepId == s.StepId).CarePlanProducts
                            .Select(cpp => new ProductDto
                            {
                                ProductId = cpp.ProductId,
                                ProductName = cpp.Product.ProductName,
                                Description = cpp.Product.Description
                            }).ToList()
                    }).ToList()
            };

            return Ok(carePlanDto);
        }

        // Phương thức gợi ý sản phẩm cho từng bước (loại bỏ random)
        private List<ProductDto> GetSuggestedProductsForStep(int? skinTypeId, int stepOrder)
        {
            // Lấy tất cả các sản phẩm phù hợp
            var products = _context.Products
                .Where(p => p.SkinTypeId == skinTypeId && p.CategoryId == stepOrder)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description
                })
                .ToList();

            return products;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCarePlan([FromBody] CreateCarePlanDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validation
                if (createDto.Steps == null || !createDto.Steps.Any())
                    return BadRequest("Care plan must have at least one step");

                // Check SkinType exists
                var skinType = await _context.SkinTypes.FindAsync(createDto.SkinTypeId);
                if (skinType == null) return BadRequest("Invalid SkinTypeId");

                // Create new CarePlan
                var carePlan = new CarePlan
                {
                    PlanName = createDto.PlanName,
                    Description = createDto.Description,
                    SkinTypeId = createDto.SkinTypeId,
                    CarePlanStep = createDto.Steps.Select(s => new CarePlanStep
                    {
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        StepDescription = s.StepDescription
                    }).ToList()
                };

                _context.CarePlans.Add(carePlan);
                await _context.SaveChangesAsync(); // Save to generate CarePlanId and StepIds

                // Process products for each step
                foreach (var step in carePlan.CarePlanStep)
                {
                    var stepDto = createDto.Steps.First(s => s.StepOrder == step.StepOrder);

                    if (stepDto.Products != null)
                    {
                        foreach (var productDto in stepDto.Products)
                        {
                            var product = await _context.Products.FindAsync(productDto.ProductId);
                            if (product == null)
                                return BadRequest($"Invalid ProductId {productDto.ProductId}");

                            var carePlanProduct = new CarePlanProduct
                            {
                                CarePlanId = carePlan.CarePlanId,
                                StepId = step.StepId,
                                ProductId = product.ProductId,
                                UserId = 0
                            };

                            _context.CarePlanProducts.Add(carePlanProduct);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var carePlanDto = await _context.CarePlans
                    .Include(cp => cp.CarePlanStep)
                        .ThenInclude(cs => cs.CarePlanProducts)
                            .ThenInclude(cpp => cpp.Product)
                    .Where(cp => cp.CarePlanId == carePlan.CarePlanId)
                    .Select(cp => new CarePlanDto
                    {
                        CarePlanId = cp.CarePlanId,
                        PlanName = cp.PlanName,
                        Description = cp.Description,
                        Steps = cp.CarePlanStep
                            .OrderBy(s => s.StepOrder)
                            .Select(s => new CarePlanStepDto
                            {
                                StepId = s.StepId,
                                StepOrder = s.StepOrder,
                                StepName = s.StepName,
                                StepDescription = s.StepDescription,
                                Products = s.CarePlanProducts
                                    .Select(cpp => new ProductDto
                                    {
                                        ProductId = cpp.ProductId,
                                        ProductName = cpp.Product.ProductName,
                                        Description = cpp.Product.Description
                                    }).ToList()
                            }).ToList()
                    }).FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetCarePlan), new { id = carePlan.CarePlanId }, carePlanDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCarePlan(int id, [FromBody] UpdateCarePlanDto updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var carePlan = await _context.CarePlans
                    .Include(cp => cp.CarePlanStep)
                        .ThenInclude(cs => cs.CarePlanProducts)
                    .FirstOrDefaultAsync(cp => cp.CarePlanId == id);

                if (carePlan == null) return NotFound();

                // Update basic info
                carePlan.PlanName = updateDto.PlanName;
                carePlan.Description = updateDto.Description;
                carePlan.SkinTypeId = updateDto.SkinTypeId;

                // Process steps
                var existingSteps = carePlan.CarePlanStep.ToList();
                var stepIdsToRemove = existingSteps.Select(s => s.StepId).ToHashSet();

                foreach (var updatedStep in updateDto.Steps)
                {
                    var existingStep = existingSteps.FirstOrDefault(s => s.StepId == updatedStep.StepId);

                    if (existingStep != null)
                    {
                        // Update step info
                        existingStep.StepOrder = updatedStep.StepOrder;
                        existingStep.StepName = updatedStep.StepName;
                        existingStep.StepDescription = updatedStep.StepDescription;

                        // Process products
                        var existingProductIds = existingStep.CarePlanProducts.Select(p => p.ProductId).ToHashSet();
                        var updatedProductIds = updatedStep.Products?.Select(p => p.ProductId).ToHashSet() ?? new HashSet<int>();

                        // Add new products
                        foreach (var productId in updatedProductIds.Except(existingProductIds))
                        {
                            var product = await _context.Products.FindAsync(productId);
                            if (product == null)
                                return BadRequest($"Invalid ProductId {productId}");

                            existingStep.CarePlanProducts.Add(new CarePlanProduct
                            {
                                ProductId = product.ProductId,
                                UserId = 0,
                                CarePlanId = id,
                                StepId = existingStep.StepId
                            });
                        }

                        // Delete removed products
                        foreach (var productId in existingProductIds.Except(updatedProductIds))
                        {
                            var productToRemove = existingStep.CarePlanProducts.FirstOrDefault(p => p.ProductId == productId);
                            if (productToRemove != null)
                            {
                                _context.CarePlanProducts.Remove(productToRemove);
                            }
                        }

                        stepIdsToRemove.Remove(existingStep.StepId); // Step is updated, don't remove
                    }
                    else
                    {
                        // Add new step
                        var newStep = new CarePlanStep
                        {
                            StepOrder = updatedStep.StepOrder,
                            StepName = updatedStep.StepName,
                            StepDescription = updatedStep.StepDescription,
                            CarePlanProducts = updatedStep.Products.Select(p => new CarePlanProduct
                            {
                                ProductId = p.ProductId
                            }).ToList()
                        };
                        carePlan.CarePlanStep.Add(newStep);
                    }
                }

                // Remove deleted steps
                foreach (var stepId in stepIdsToRemove)
                {
                    var stepToRemove = existingSteps.FirstOrDefault(s => s.StepId == stepId);
                    if (stepToRemove != null)
                    {
                        _context.CarePlanSteps.Remove(stepToRemove);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(carePlan);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarePlan(int id)
        {
            var carePlan = await _context.CarePlans
                .Include(cp => cp.CarePlanProducts)
                .Include(cp => cp.CarePlanStep)
                .Include(cp => cp.UserCarePlans)
                .FirstOrDefaultAsync(cp => cp.CarePlanId == id);

            if (carePlan == null)
            {
                return NotFound();
            }

            _context.CarePlanProducts.RemoveRange(carePlan.CarePlanProducts);
            _context.CarePlanSteps.RemoveRange(carePlan.CarePlanStep);
            _context.UserCarePlans.RemoveRange(carePlan.UserCarePlans);

            try
            {
                _context.CarePlans.Remove(carePlan);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the CarePlan.");
            }
        }
    }

    // DTOs để trả về dữ liệu có cấu trúc
    public class CarePlanDto
    {
        public int CarePlanId { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public List<CarePlanStepDto> Steps { get; set; }
    }

    public class CarePlanStepDto
    {
        public int StepId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public string StepDescription { get; set; }
        public List<ProductDto> Products { get; set; }
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
    }

    // DTO mới cho việc tạo Care Plan - phù hợp với định dạng JSON đầu vào
    public class CreateCarePlanDto
    {
        public string PlanName { get; set; }
        public string Description { get; set; }
        public int SkinTypeId { get; set; }
        public List<CreateCarePlanStepDto> Steps { get; set; }
    }

    public class CreateCarePlanStepDto
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public string StepDescription { get; set; }
        public List<CreateProductDto> Products { get; set; }
    }

    public class CreateProductDto
    {
        public int ProductId { get; set; } // Only ProductId is needed
    }
    // DTO cho cập nhật
    public class UpdateCarePlanDto
    {
        public string PlanName { get; set; }
        public string Description { get; set; }
        public int SkinTypeId { get; set; }
        public List<UpdateCarePlanStepDto> Steps { get; set; }
    }

    public class UpdateProductDto
    {
        public int ProductId { get; set; }
    }

    public class UpdateCarePlanStepDto
    {
        public int StepId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public List<UpdateProductDto> Products { get; set; }
        public string StepDescription { get; set; }
    }
}