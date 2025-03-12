using BeautySky.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CarePlanController : ControllerBase
{
    private readonly ProjectSwpContext _context;

    public CarePlanController(ProjectSwpContext context)
    {
        _context = context;
    }

    [HttpPost("GetCarePlan")]
    public IActionResult GetCarePlan(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound("User not found");

        // Lấy quiz gần nhất của user
        var userQuiz = _context.UserQuizzes
            .Where(uq => uq.UserId == userId)
            .OrderByDescending(uq => uq.DateTaken) // Lấy quiz gần nhất
            .FirstOrDefault();

        if (userQuiz == null)
            return NotFound("No quiz found for this user");

        // Lấy UserAnswer tương ứng với quiz gần nhất
        var userAnswer = _context.UserAnswers
            .Where(ua => ua.UserQuizId == userQuiz.UserQuizId)
            .OrderByDescending(ua => ua.UserAnswerId) // Lấy UserAnswer gần nhất nếu có nhiều câu trả lời
            .FirstOrDefault();

        if (userAnswer == null)
            return NotFound("No answer found for this user");

        // Lấy SkinTypeId từ UserAnswer của quiz gần nhất
        int skinTypeId = userAnswer.SkinTypeId ?? 0;

        // Lấy CarePlan dựa trên SkinTypeId của quiz gần nhất
        var carePlan = _context.CarePlans.FirstOrDefault(cp => cp.SkinTypeId == skinTypeId);
        if (carePlan == null)
            return NotFound("No care plan found for this skin type");

        SaveUserCarePlan(userId, carePlan.CarePlanId);

        var steps = _context.CarePlanSteps.Where(s => s.CarePlanId == carePlan.CarePlanId).OrderBy(s => s.StepOrder).ToList();
        var result = new
        {
            carePlan.PlanName,
            carePlan.Description,
            Steps = steps.Select(step => new
            {
                step.StepOrder,
                step.StepName,
                Products = GetRandomProductForStep(userAnswer.SkinTypeId, step.StepOrder) // Lấy 1 sản phẩm ngẫu nhiên cho mỗi bước
            }).ToList()
        };

        return Ok(result);
    }

    private List<object> GetRandomProductForStep(int? skinTypeId, int stepOrder)
    {
        // Kiểm tra skinTypeId có null không, nếu null thì thay bằng 0 (hoặc giá trị mặc định bạn muốn)
        int validSkinTypeId = skinTypeId ?? 0;

        // Lấy sản phẩm phù hợp với SkinTypeId và StepOrder (CategoryId trong bảng Products tương ứng với StepOrder)
        var products = _context.Products
            .Where(p => p.SkinTypeId == validSkinTypeId && p.CategoryId == stepOrder) // Dựa vào StepOrder để chọn đúng category
            .ToList();

        // Nếu có sản phẩm phù hợp, chọn 1 sản phẩm ngẫu nhiên
        if (products.Any())
        {
            var randomProduct = products.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            return new List<object> // Trả về một danh sách chứa 1 sản phẩm
        {
            new
            {
                randomProduct.ProductId,
                randomProduct.ProductName
            }
        };
        }

        return new List<object>(); // Nếu không có sản phẩm, trả về danh sách rỗng
    }

    private void SaveUserCarePlan(int userId, int carePlanId)
    {
        // Lấy quiz mới nhất của user
        var userQuiz = _context.UserQuizzes
            .Where(uq => uq.UserId == userId)
            .OrderByDescending(uq => uq.DateTaken)
            .FirstOrDefault();

        if (userQuiz == null) return;

        // Lấy loại da mới nhất từ UserAnswer
        var userAnswer = _context.UserAnswers
            .Where(ua => ua.UserQuizId == userQuiz.UserQuizId)
            .OrderByDescending(ua => ua.UserAnswerId)
            .FirstOrDefault();

        if (userAnswer == null) return;

        int newSkinTypeId = userAnswer.SkinTypeId ?? 0; // Loại da mới nhất

        // Xóa lộ trình cũ của user nếu có
        var oldCarePlanProducts = _context.CarePlanProducts
            .Where(cp => cp.UserId == userId)
            .ToList();
        _context.CarePlanProducts.RemoveRange(oldCarePlanProducts);

        var oldUserCarePlan = _context.UserCarePlans
            .Where(u => u.UserId == userId)
            .ToList();
        _context.UserCarePlans.RemoveRange(oldUserCarePlan);

        _context.SaveChanges();

        // Tạo lộ trình mới
        var userCarePlan = new UserCarePlan
        {
            UserId = userId,
            CarePlanId = carePlanId,
            DateCreate = DateTime.Now
        };
        _context.UserCarePlans.Add(userCarePlan);
        _context.SaveChanges();

        var steps = GetStepsByCarePlanId(carePlanId);
        var random = new Random();

        foreach (var step in steps)
        {
            // Lấy danh sách sản phẩm có cùng loại da và category
            var products = _context.Products
                .Where(p => p.SkinTypeId == newSkinTypeId && p.CategoryId == step.StepOrder)
                .ToList();

            if (products.Any())
            {
                // Chọn sản phẩm ngẫu nhiên phù hợp với loại da mới
                var randomProduct = products.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                var carePlanProduct = new CarePlanProduct
                {
                    CarePlanId = carePlanId,
                    ProductId = randomProduct.ProductId, // Sản phẩm mới đúng loại da
                    ProductName = randomProduct.ProductName,
                    StepId = step.StepId,
                    UserId = userId
                };

                // Chỉ thêm sản phẩm mới nếu chưa có
                if (!_context.CarePlanProducts.Any(cp => cp.UserId == userId && cp.ProductId == randomProduct.ProductId && cp.StepId == step.StepId))
                {
                    _context.CarePlanProducts.Add(carePlanProduct);
                }
            }
        }
        _context.SaveChanges();
    }



    // Lấy tất cả các bước của care plan theo CarePlanId
    private List<CarePlanStep> GetStepsByCarePlanId(int carePlanId)
    {
        return _context.CarePlanSteps.Where(s => s.CarePlanId == carePlanId).ToList();
    }
}
