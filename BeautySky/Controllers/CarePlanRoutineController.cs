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


    [HttpGet("GetUserCarePlan/{userId}")]
    public async Task<IActionResult> GetUserCarePlan(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var userQuiz = await _context.UserQuizzes
            .Where(uq => uq.UserId == userId)
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

        int skinTypeId = userAnswer.SkinTypeId ?? 0;

        var carePlan = await _context.CarePlans
            .Include(cp => cp.CarePlanSteps)
                .ThenInclude(cps => cps.CarePlanProducts)
            .FirstOrDefaultAsync(cp => cp.SkinTypeId == skinTypeId);

        if (carePlan == null)
        {
            return NotFound("No care plan found for this skin type");
        }

        var result = new
        {
            carePlan.PlanName,
            carePlan.Description,
            Steps = carePlan.CarePlanSteps.OrderBy(s => s.StepOrder).Select(step => new
            {
                step.StepOrder,
                step.StepName,
                Products = step.CarePlanProducts
                    .Where(cpp => cpp.CarePlanId == carePlan.CarePlanId && cpp.UserId == userId) // Lọc theo UserId
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
        };

        return Ok(result);
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
        var oldCarePlanProducts = _context.CarePlanProduct
            .Where(cp => cp.UserId == request.UserId)
            .ToList();
        _context.CarePlanProduct.RemoveRange(oldCarePlanProducts);

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
            DateCreate = DateTime.Now
        };
        _context.UserCarePlans.Add(userCarePlan);
        await _context.SaveChangesAsync();

        var steps = GetStepsByCarePlanId(request.CarePlanId);

        foreach (var step in steps)
        {
            var products = _context.Products
                .Where(p => p.SkinTypeId == request.SkinTypeId && p.CategoryId == step.StepOrder)
                .ToList();

            if (products.Any())
            {
                var randomProduct = products.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                var carePlanProduct = new CarePlanProduct
                {
                    CarePlanId = request.CarePlanId,
                    ProductId = randomProduct.ProductId,
                    ProductName = randomProduct.ProductName,
                    StepId = step.StepId,
                    UserId = request.UserId
                };

                if (!_context.CarePlanProduct.Any(cp => cp.UserId == request.UserId && cp.ProductId == randomProduct.ProductId && cp.StepId == step.StepId))
                {
                    _context.CarePlanProduct.Add(carePlanProduct);
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

        // Xóa lộ trình cũ trước khi lưu mới
        DeleteOldUserCarePlan(userId);

        // Lưu lộ trình mới vào bảng UserCarePlan
        SaveUserCarePlan(userId, carePlan.CarePlanId);

        List<object> stepResults = new List<object>();
        foreach (var step in steps)
        {
            var products = _context.Products
                .Where(p => p.SkinTypeId == skinTypeId && p.CategoryId == step.StepOrder)
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
                var carePlanProduct = new CarePlanProduct
                {
                    CarePlanId = carePlan.CarePlanId,
                    ProductId = randomProduct.ProductId,
                    ProductName = randomProduct.ProductName,
                    StepId = step.StepId,
                    UserId = userId
                };
                _context.CarePlanProduct.Add(carePlanProduct);

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

    private void DeleteOldUserCarePlan(int userId)
    {
        var oldCarePlanProducts = _context.CarePlanProduct
            .Where(cp => cp.UserId == userId)
            .ToList();
        _context.CarePlanProduct.RemoveRange(oldCarePlanProducts);

        var oldUserCarePlan = _context.UserCarePlans
            .Where(u => u.UserId == userId)
            .ToList();
        _context.UserCarePlans.RemoveRange(oldUserCarePlan);

        _context.SaveChanges();
    }

    private void SaveUserCarePlan(int userId, int carePlanId)
    {
        var userCarePlan = new UserCarePlan
        {
            UserId = userId,
            CarePlanId = carePlanId,
            DateCreate = DateTime.Now
        };
        _context.UserCarePlans.Add(userCarePlan);
        _context.SaveChanges();
    }



    // Lấy tất cả các bước của care plan theo CarePlanId
    private List<CarePlanStep> GetStepsByCarePlanId(int carePlanId)
    {
        return _context.CarePlanSteps.Where(s => s.CarePlanId == carePlanId).ToList();
    }
}
