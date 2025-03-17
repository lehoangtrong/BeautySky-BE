using BeautySky.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CarePlanRoutineController : ControllerBase
{
    private readonly ProjectSwpContext _context;

    public CarePlanRoutineController(ProjectSwpContext context)
    {
        _context = context;
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

    private void DeleteOldUserCarePlan(int userId)
    {
        var oldCarePlanProducts = _context.CarePlanProducts
            .Where(cp => cp.UserId == userId)
            .ToList();
        _context.CarePlanProducts.RemoveRange(oldCarePlanProducts);

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
}
