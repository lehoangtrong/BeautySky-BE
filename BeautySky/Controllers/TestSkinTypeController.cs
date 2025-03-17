using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySky.Models;
using BeautySky.DTO;

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

        [HttpPost("submit-quiz")]
        public async Task<IActionResult> SubmitQuiz([FromBody] List<UserAnswerDTO> userAnswers)
        {
            var result = new Dictionary<int, double>();
            List<string> questionIds = new();
            List<string> answerIds = new();

            if (userAnswers == null || !userAnswers.Any())
            {
                return BadRequest("Danh sách câu trả lời không được trống.");
            }

            int? userIdParsed = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst("userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parseId))
                {
                    userIdParsed = parseId;
                }
            }

            // Lấy QuizId từ câu hỏi đầu tiên
            var firstQuestion = await _context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == userAnswers[0].QuestionId);
            if (firstQuestion == null)
            {
                return BadRequest("Không tìm thấy câu hỏi.");
            }
            var quizId = firstQuestion.QuizId;

            UserQuiz? userQuiz = null;

            // Nếu có userId thì lưu vào DB
            if (userIdParsed != null)
            {
                userQuiz = new UserQuiz
                {
                    UserId = userIdParsed,
                    QuizId = quizId
                };
                _context.UserQuizzes.Add(userQuiz);
                await _context.SaveChangesAsync();
            }

            foreach (var userAnswerDto in userAnswers)
            {
                var answerRecord = await _context.Answers.FirstOrDefaultAsync(a => a.AnswerId == userAnswerDto.AnswerId);
                if (answerRecord != null)
                {
                    questionIds.Add(userAnswerDto.QuestionId.ToString());
                    answerIds.Add(userAnswerDto.AnswerId.ToString());

                    var skinTypes = !string.IsNullOrEmpty(answerRecord.SkinTypeId) ? answerRecord.SkinTypeId.Split(',') : Array.Empty<string>();
                    var points = !string.IsNullOrEmpty(answerRecord.Point) ? answerRecord.Point.Split(',') : Array.Empty<string>();

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

            return Ok(new
            {
                SkinTypeResults = skinTypeResults,
                BestSkinType = new
                {
                    SkinTypeId = highestScoreSkinType.Key,
                    SkinTypeName = highestScoreSkinType.Key > 0 ? (await _context.SkinTypes.FirstOrDefaultAsync(st => st.SkinTypeId == highestScoreSkinType.Key))?.SkinTypeName : "Không xác định"
                },
                UserQuizId = userQuiz?.UserQuizId
            });
        }

    }
}