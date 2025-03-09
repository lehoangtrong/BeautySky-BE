using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeautySky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSkinTypeController : ControllerBase
    {
        private readonly ProjectSwpContext _context;

        public TestSkinTypeController(ProjectSwpContext context)
        {
            _context = context;
        }

        [HttpPost("SubmitQuiz")]
        public async Task<IActionResult> SubmitQuiz([FromBody] List<UserAnswerDto> userAnswers)
        {
            if (userAnswers == null || !userAnswers.Any())
            {
                return BadRequest("Danh sách câu trả lời không được trống.");
            }

            var result = new Dictionary<int, double>();
            List<string> questionIds = new();
            List<string> answerIds = new();

            int? userIdParsed = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst("userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                {
                    userIdParsed = parsedId;
                }
            }

            var firstQuestion = await _context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == userAnswers[0].QuestionId);
            if (firstQuestion == null)
            {
                return BadRequest("Không tìm thấy câu hỏi.");
            }
            var quizId = firstQuestion.QuizId;

            UserQuiz? userQuiz = null;
            if (userIdParsed != null)
            {
                userQuiz = new UserQuiz
                {
                    UserId = userIdParsed,
                    QuizId = quizId,
                    DateTaken = DateTime.Now
                };
                _context.UserQuizzes.Add(userQuiz);
                await _context.SaveChangesAsync();
            }

            foreach (var userAnswerDto in userAnswers)
            {
                var answerRecord = await _context.Answers
                    .FirstOrDefaultAsync(a => a.AnswerId == userAnswerDto.AnswerId);
                if (answerRecord != null)
                {
                    questionIds.Add(userAnswerDto.QuestionId.ToString());
                    answerIds.Add(userAnswerDto.AnswerId.ToString());

                    var skinTypes = !string.IsNullOrEmpty(answerRecord.SkinTypeId)
                        ? answerRecord.SkinTypeId.Split(',')
                        : Array.Empty<string>();
                    var points = !string.IsNullOrEmpty(answerRecord.Point)
                        ? answerRecord.Point.Split(',')
                        : Array.Empty<string>();

                    if (skinTypes.Length == points.Length)
                    {
                        for (int i = 0; i < skinTypes.Length; i++)
                        {
                            if (int.TryParse(skinTypes[i], out int skinTypeId) && int.TryParse(points[i], out int point))
                            {
                                if (!result.ContainsKey(skinTypeId))
                                {
                                    result[skinTypeId] = 0;
                                }
                                result[skinTypeId] += point;
                            }
                        }
                    }
                }
            }

            var highestScoreSkinType = result.OrderByDescending(r => r.Value).FirstOrDefault();

            if (userIdParsed != null && userQuiz != null)
            {
                var userAnswer = new UserAnswer
                {
                    UserQuizId = userQuiz.UserQuizId,
                    QuestionId = string.Join(",", questionIds),
                    AnswerId = string.Join(",", answerIds),
                    SkinTypeId = highestScoreSkinType.Key > 0 ? highestScoreSkinType.Key : (int?)null
                };
                _context.UserAnswers.Add(userAnswer);
                await _context.SaveChangesAsync();
            }

            var skinTypeResults = await _context.SkinTypes
                .Select(st => new
                {
                    SkinTypeId = st.SkinTypeId,
                    SkinTypeName = st.SkinTypeName,
                    Points = result.ContainsKey(st.SkinTypeId) ? result[st.SkinTypeId] : 0
                }).ToListAsync();

            var carePlanEntity = await _context.CarePlans
                .Include(cp => cp.CarePlanStep)
                .FirstOrDefaultAsync(cp => cp.SkinTypeId == highestScoreSkinType.Key);

            CarePlanDto carePlanDto = null;
            if (carePlanEntity != null)
            {
                carePlanDto = new CarePlanDto
                {
                    CarePlanId = carePlanEntity.CarePlanId,
                    PlanName = carePlanEntity.PlanName,
                    Description = carePlanEntity.Description,
                    Steps = carePlanEntity.CarePlanStep
                        .OrderBy(s => s.StepOrder)
                        .Select(s => new CarePlanStepDto
                        {
                            StepId = s.StepId,
                            StepOrder = s.StepOrder,
                            StepName = s.StepName,
                            StepDescription = s.StepDescription,
                            Products = GetProductsForStep(carePlanEntity.CarePlanId, s.StepId)
                        }).ToList()
                };
            }

            return Ok(new
            {
                SkinTypeResults = skinTypeResults,
                BestSkinType = new
                {
                    SkinTypeId = highestScoreSkinType.Key,
                    SkinTypeName = highestScoreSkinType.Key > 0
                        ? (await _context.SkinTypes.FirstOrDefaultAsync(st => st.SkinTypeId == highestScoreSkinType.Key))?.SkinTypeName
                        : "Không xác định"
                },
                CarePlan = carePlanDto,
                UserQuizId = userQuiz?.UserQuizId
            });
        }

        private List<ProductDto> GetProductsForStep(int carePlanId, int stepId)
        {
            var carePlanProducts = _context.CarePlanProducts
                .Where(cp => cp.CarePlanId == carePlanId && cp.StepId == stepId)
                .Include(cp => cp.Product)
                .ToList();

            return carePlanProducts.Select(cp => new ProductDto
            {
                ProductId = cp.Product.ProductId,
                ProductName = cp.Product.ProductName,
                Description = cp.Product.Description
            }).ToList();
        }
    }

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

    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
}