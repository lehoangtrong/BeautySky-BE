using BeautySky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CarePlanController : ControllerBase
{
    private readonly ProjectSwpContext _context;

    public CarePlanController(ProjectSwpContext context)
    {
        _context = context;
    }


    [HttpGet("GetUserCarePlans/{userId}")]
    public async Task<IActionResult> GetUserCarePlans(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Lấy tất cả lộ trình của user
        var userCarePlans = await _context.UserCarePlans
            .Where(ucp => ucp.UserId == userId)
            .Select(ucp => new
            {
                ucp.CarePlan.CarePlanId,
                ucp.CarePlan.PlanName,
                ucp.CarePlan.Description,
                ucp.DateCreate, // Ngày tạo lộ trình
                Steps = ucp.CarePlan.CarePlanSteps
                    .OrderBy(s => s.StepOrder)
                    .Select(step => new
                    {
                        step.StepOrder,
                        step.StepName,
                        Products = step.CarePlanProducts
                            .Where(cpp => cpp.CarePlanId == ucp.CarePlanId && cpp.UserId == userId) // Lọc theo UserId
                            .Select(cpp => new
                            {
                                ProductId = cpp.ProductId,
                                ProductName = cpp.ProductName,
                                ProductImage = _context.ProductsImages
                                    .Where(img => img.ProductId == cpp.ProductId)
                                    .Select(img => img.ImageUrl)
                                    .FirstOrDefault(),
                                ProductPrice = _context.Products
                                    .Where(p => p.ProductId == cpp.ProductId)
                                    .Select(p => p.Price)
                                    .FirstOrDefault()
                            }).ToList()
                    }).ToList()
            })
            .OrderByDescending(ucp => ucp.DateCreate) // Sắp xếp theo ngày tạo mới nhất
            .ToListAsync();

        if (!userCarePlans.Any())
        {
            return NotFound("No care plans found for this user");
        }

        return Ok(userCarePlans);
    }




    [HttpPost("SaveUserCarePlan")]
    public async Task<IActionResult> SaveUserCarePlan([FromBody] SaveCarePlanRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        // Kiểm tra request
        if (request == null || request.UserId <= 0)
        {
            return BadRequest("Invalid request data");
        }

        // Lấy thông tin quiz và skin type của user
        var userQuiz = await _context.UserQuizzes
            .Where(uq => uq.UserId == request.UserId)
            .OrderByDescending(uq => uq.DateTaken)
            .FirstOrDefaultAsync();

        if (userQuiz == null)
        {
            return NotFound("No quiz found for this user");
        }

        var userAnswer = await _context.UserAnswers
            .Where(ua => ua.UserQuizId == userQuiz.UserQuizId)
            .OrderByDescending(ua => ua.UserAnswerId)
            .FirstOrDefaultAsync();

        if (userAnswer == null)
        {
            return NotFound("No answer found for this user");
        }

        // Lấy care plan dựa trên skin type
        var carePlan = await _context.CarePlans
            .FirstOrDefaultAsync(cp => cp.SkinTypeId == userAnswer.SkinTypeId);

        if (carePlan == null)
        {
            return NotFound("No care plan found for this skin type");
        }

        // Cập nhật request với thông tin đúng
        request.CarePlanId = carePlan.CarePlanId;
        request.SkinTypeId = userAnswer.SkinTypeId ?? 0;

        // Xóa lộ trình cũ của user nếu có
        var oldCarePlanProducts = _context.CarePlanProducts
            .Where(cp => cp.UserId == request.UserId)
            .ToList();
        _context.CarePlanProducts.RemoveRange(oldCarePlanProducts);

        var oldUserCarePlan = _context.UserCarePlans
            .Where(u => u.UserId == request.UserId)
            .ToList();
        _context.UserCarePlans.RemoveRange(oldUserCarePlan);

        await _context.SaveChangesAsync();

        // Tạo lộ trình mới
        var userCarePlan = new UserCarePlan
        {
            UserId = request.UserId,
            CarePlanId = request.CarePlanId,
            DateCreate = DateTime.UtcNow
        };
        _context.UserCarePlans.Add(userCarePlan);
        await _context.SaveChangesAsync();

        var steps = GetStepsByCarePlanId(request.CarePlanId);

        foreach (var step in steps)
        {
            var products = _context.Products
                .Where(p => p.SkinTypeId == request.SkinTypeId && p.CategoryId == step.StepOrder && p.IsActive) // Thêm && p.IsActive để check sản phẩm nào có Active là true thì mới dc random còn false thì ko dc random
                .ToList();

            if (products.Any())
            {
                var randomProduct = products.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                var carePlanProduct = new CarePlanProducts
                {
                    CarePlanId = request.CarePlanId,
                    ProductId = randomProduct.ProductId,
                    ProductName = randomProduct.ProductName,
                    StepId = step.StepId,
                    UserId = request.UserId
                };

                if (!_context.CarePlanProducts.Any(cp => cp.UserId == request.UserId && cp.ProductId == randomProduct.ProductId && cp.StepId == step.StepId))
                {
                    _context.CarePlanProducts.Add(carePlanProduct);
                }
            }
        }
        await _context.SaveChangesAsync();

        return Ok("Lộ trình đã được lưu thành công!");
    }

    [HttpPost("GetCarePlan")]
    public IActionResult GetCarePlan(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound("User not found");

        var userQuiz = _context.UserQuizzes
            .Where(uq => uq.UserId == userId)
            .OrderByDescending(uq => uq.DateTaken)
            .FirstOrDefault();

        if (userQuiz == null)
            return NotFound("No quiz found for this user");

        var userAnswer = _context.UserAnswers
            .Where(ua => ua.UserQuizId == userQuiz.UserQuizId)
            .OrderByDescending(ua => ua.UserAnswerId)
            .FirstOrDefault();

        if (userAnswer == null)
            return NotFound("No answer found for this user");

        int skinTypeId = userAnswer.SkinTypeId ?? 0;

        var carePlan = _context.CarePlans.FirstOrDefault(cp => cp.SkinTypeId == skinTypeId);
        if (carePlan == null)
            return NotFound("No care plan found for this skin type");

        var steps = _context.CarePlanSteps
            .Where(s => s.CarePlanId == carePlan.CarePlanId)
            .OrderBy(s => s.StepOrder)
            .ToList();

        // Nếu loại da thay đổi, tạo lộ trình mới cho user
        SaveUserCarePlan(userId, carePlan.CarePlanId, skinTypeId);

        List<object> stepResults = new List<object>();
        foreach (var step in steps)
        {
            var products = _context.Products
                .Where(p => p.SkinTypeId == skinTypeId && p.CategoryId == step.StepOrder && p.IsActive) // Thêm && p.IsActive để check sản phẩm nào có Active là true thì mới dc random còn false thì ko dc random
                .ToList();

            if (products.Any())
            {
                var randomProduct = products.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                // Lấy hình ảnh sản phẩm (lấy ảnh đầu tiên nếu có)
                var productImage = _context.ProductsImages
                    .Where(pi => pi.ProductId == randomProduct.ProductId)
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault();

                // Lưu vào DB ngay khi chọn
                var carePlanProduct = new CarePlanProducts
                {
                    CarePlanId = carePlan.CarePlanId,
                    ProductId = randomProduct.ProductId,
                    ProductName = randomProduct.ProductName,
                    StepId = step.StepId,
                    UserId = userId
                };
                _context.CarePlanProducts.Add(carePlanProduct);

                stepResults.Add(new
                {
                    step.StepOrder,
                    step.StepName,
                    Products = new List<object>
                {
                    new
                    {
                        randomProduct.ProductId,
                        randomProduct.ProductName,
                        randomProduct.Price,   // Thêm giá sản phẩm
                        ProductImage  = productImage ?? ""  // Thêm ảnh sản phẩm
                    }
                }
                });
            }
        }

        _context.SaveChanges(); // Lưu toàn bộ sản phẩm sau khi chọn xong

        var result = new
        {
            carePlan.PlanName,
            carePlan.Description,
            Steps = stepResults
        };

        return Ok(result);
    }

    [HttpDelete("DeleteUserCarePlan/{userId}")]
    public async Task<IActionResult> DeleteUserCarePlan(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            // Lấy tất cả lộ trình của user
            var userCarePlans = await _context.UserCarePlans
                .Where(ucp => ucp.UserId == userId)
                .ToListAsync();

            if (!userCarePlans.Any())
            {
                return NotFound("Không tìm thấy lộ trình nào của người dùng này");
            }

            // Xóa tất cả sản phẩm trong lộ trình
            var carePlanProducts = await _context.CarePlanProducts
                .Where(cpp => cpp.UserId == userId)
                .ToListAsync();

            _context.CarePlanProducts.RemoveRange(carePlanProducts);

            // Xóa lộ trình
            _context.UserCarePlans.RemoveRange(userCarePlans);

            await _context.SaveChangesAsync();

            return Ok("Đã xóa lộ trình thành công!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi xóa lộ trình: {ex.Message}");
        }
    }

    private async Task SaveUserCarePlan(int userId, int carePlanId, int skinTypeId)
    {
        var existingCarePlan = await _context.UserCarePlans
            .FirstOrDefaultAsync(u => u.UserId == userId && u.CarePlan.SkinTypeId == skinTypeId);

        if (existingCarePlan != null)
        {
            // Cập nhật ngày tạo mới
            existingCarePlan.DateCreate = DateTime.UtcNow; // Sử dụng UTC time

            // Xóa các sản phẩm cũ trong lộ trình
            var oldCarePlanProducts = await _context.CarePlanProducts
                .Where(cp => cp.UserId == userId && cp.CarePlanId == existingCarePlan.CarePlanId)
                .ToListAsync();

            _context.CarePlanProducts.RemoveRange(oldCarePlanProducts);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Tạo lộ trình mới với ngày tạo hiện tại
            var userCarePlan = new UserCarePlan
            {
                UserId = userId,
                CarePlanId = carePlanId,
                DateCreate = DateTime.UtcNow // Sử dụng UTC time
            };
            _context.UserCarePlans.Add(userCarePlan);
            await _context.SaveChangesAsync();
        }
    }

    // Lấy tất cả các bước của care plan theo CarePlanId
    private List<CarePlanStep> GetStepsByCarePlanId(int carePlanId)
    {
        return _context.CarePlanSteps.Where(s => s.CarePlanId == carePlanId).ToList();
    }
}
