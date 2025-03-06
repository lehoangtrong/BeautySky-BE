//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using BeautySky.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Extensions.Caching.Memory;

//namespace BeautySky.Controllers
//{
//    [Route("api/routine")]
//    [ApiController]
//    public class RoutineController : ControllerBase
//    {
//        private readonly ProjectSwpContext _context;
//        private readonly IMemoryCache _cache;

//        public RoutineController(ProjectSwpContext context, IMemoryCache cache)
//        {
//            _context = context;
//            _cache = cache;
//        }

//        // **1. Lấy danh sách câu hỏi**
//        [HttpGet("questions")]
//        public ActionResult<IEnumerable<Question>> GetQuestions()
//        {
//            if (!_cache.TryGetValue("QuizQuestions", out IEnumerable<Question> questions))
//            {
//                var quiz = _context.Quizzes
//                    .Include(q => q.Questions)
//                    .ThenInclude(q => q.Answers)
//                    .FirstOrDefault();

//                if (quiz == null)
//                {
//                    return NotFound("Không tìm thấy quiz.");
//                }

//                questions = quiz.Questions;
//                _cache.Set("QuizQuestions", questions, TimeSpan.FromHours(1));
//            }

//            return Ok(questions);
//        }

//        // **DTO để nhận câu trả lời từ Front-end**
//        public class UserAnswerDto
//        {
//            public int QuestionId { get; set; }
//            public int AnswerId { get; set; }
//        }

//        // **2. Gửi câu trả lời và nhận kết quả**
//        [HttpPost("submit")]
//        public ActionResult SubmitAnswers([FromBody] List<UserAnswerDto> userAnswers)
//        {
//            if (userAnswers == null || userAnswers.Count != 5)
//            {
//                return BadRequest("Vui lòng trả lời đầy đủ 5 câu hỏi.");
//            }

//            // Tính điểm cho từng loại da
//            var skinTypeScores = CalculateSkinTypeScores(userAnswers);
//            var dominantSkinTypeId = skinTypeScores.OrderByDescending(s => s.Value).First().Key;

//            // Lưu UserQuiz
//            var userQuiz = new UserQuiz
//            {
//                UserId = GetCurrentUserId(), // Giả định lấy từ authentication
//                QuizId = 1, // Giả định quiz có ID = 1
//                DateTaken = DateTime.Now
//            };
//            _context.UserQuizzes.Add(userQuiz);
//            _context.SaveChanges();

//            // Lưu UserAnswer
//            foreach (var ua in userAnswers)
//            {
//                _context.UserAnswers.Add(new UserAnswer
//                {
//                    UserQuizId = userQuiz.UserQuizId,
//                    QuestionId = ua.QuestionId.ToString(),
//                    AnswerId = ua.AnswerId.ToString(),
//                    SkinTypeId = dominantSkinTypeId // Lưu loại da cuối cùng
//                });
//            }
//            _context.SaveChanges();

//            // Lấy CarePlan cho loại da
//            var carePlan = _context.CarePlans
//                .Include(cp => cp.CarePlanSteps)
//                .ThenInclude(cps => cps.CarePlanProducts)
//                .FirstOrDefault(cp => cp.SkinTypeId == dominantSkinTypeId);

//            if (carePlan == null)
//            {
//                return NotFound("Không tìm thấy lộ trình chăm sóc da cho loại da này.");
//            }

//            // Lưu UserCarePlan
//            var userCarePlan = new UserCarePlan
//            {
//                UserId = userQuiz.UserId,
//                CarePlanId = carePlan.CarePlanId,
//                DateCreate = DateTime.Now
//            };
//            _context.UserCarePlans.Add(userCarePlan);
//            _context.SaveChanges();

//            // Trả về kết quả
//            var skinType = _context.SkinTypes.FirstOrDefault(st => st.SkinTypeId == dominantSkinTypeId);
//            return Ok(new
//            {
//                SkinType = new { skinType.SkinTypeId, SkinTypeName = skinType.SkinTypeName },
//                CarePlan = new
//                {
//                    carePlan.CarePlanId,
//                    CarePlanName = carePlan.PlanName,
//                    Steps = carePlan.CarePlanSteps.Select(cps => new
//                    {
//                        cps.StepId,
//                        cps.StepOrder,
//                        cps.StepName,
//                        cps.StepDescription,
//                        Products = cps.CarePlanProducts.Select(p => new
//                        {
//                            ProductId = p.ProductId,
//                            ProductName = p.ProductName
//                        })
//                    })
//                }
//            });
//        }

//        // **Logic tính điểm dựa trên bộ câu hỏi**
//        private Dictionary<int, int> CalculateSkinTypeScores(List<UserAnswerDto> userAnswers)
//        {
//            var scores = new Dictionary<int, int>
//            {
//                { 1, 0 }, // Da dầu
//                { 2, 0 }, // Da khô
//                { 3, 0 }, // Da thường
//                { 4, 0 }, // Da hỗn hợp
//                { 5, 0 }  // Da nhạy cảm
//            };

//            foreach (var ua in userAnswers)
//            {
//                var answerScores = GetAnswerScores(ua.QuestionId, ua.AnswerId);
//                foreach (var score in answerScores)
//                {
//                    scores[score.Key] += score.Value;
//                }
//            }

//            return scores;
//        }

//        // **Hardcode logic điểm số dựa trên bộ câu hỏi**
//        private Dictionary<int, int> GetAnswerScores(int questionId, int answerId)
//        {
//            switch (questionId)
//            {
//                case 1:
//                    switch (answerId)
//                    {
//                        case 1: return new Dictionary<int, int> { { 1, 3 }, { 4, 1 } }; // A
//                        case 2: return new Dictionary<int, int> { { 2, 3 }, { 5, 1 } }; // B
//                        case 3: return new Dictionary<int, int> { { 3, 3 } };          // C
//                        case 4: return new Dictionary<int, int> { { 4, 3 } };          // D
//                        case 5: return new Dictionary<int, int> { { 5, 3 } };          // E
//                    }
//                    break;
//                case 2:
//                    switch (answerId)
//                    {
//                        case 1: return new Dictionary<int, int> { { 1, 3 }, { 4, 1 } }; // A
//                        case 2: return new Dictionary<int, int> { { 2, 3 }, { 5, 1 } }; // B
//                        case 3: return new Dictionary<int, int> { { 3, 3 } };          // C
//                        case 4: return new Dictionary<int, int> { { 4, 3 }, { 1, 1 } }; // D
//                        case 5: return new Dictionary<int, int> { { 5, 3 } };          // E
//                    }
//                    break;
//                case 3:
//                    switch (answerId)
//                    {
//                        case 1: return new Dictionary<int, int> { { 1, 3 }, { 5, 1 } }; // A
//                        case 2: return new Dictionary<int, int> { { 2, 3 } };          // B
//                        case 3: return new Dictionary<int, int> { { 3, 3 } };          // C
//                        case 4: return new Dictionary<int, int> { { 4, 3 } };          // D
//                        case 5: return new Dictionary<int, int> { { 5, 3 } };          // E
//                    }
//                    break;
//                case 4:
//                    switch (answerId)
//                    {
//                        case 1: return new Dictionary<int, int> { { 1, 3 } };          // A
//                        case 2: return new Dictionary<int, int> { { 2, 3 }, { 5, 1 } }; // B
//                        case 3: return new Dictionary<int, int> { { 3, 3 } };          // C
//                        case 4: return new Dictionary<int, int> { { 4, 3 }, { 1, 1 } }; // D
//                        case 5: return new Dictionary<int, int> { { 5, 3 } };          // E
//                    }
//                    break;
//                case 5:
//                    switch (answerId)
//                    {
//                        case 1: return new Dictionary<int, int> { { 1, 3 } };          // A
//                        case 2: return new Dictionary<int, int> { { 2, 3 }, { 5, 1 } }; // B
//                        case 3: return new Dictionary<int, int> { { 3, 3 } };          // C
//                        case 4: return new Dictionary<int, int> { { 4, 3 }, { 1, 1 } }; // D
//                        case 5: return new Dictionary<int, int> { { 5, 3 } };          // E
//                    }
//                    break;
//            }
//            return new Dictionary<int, int>();
//        }

//        // **Giả định hàm lấy UserId từ authentication**
//        private int GetCurrentUserId()
//        {
//            // Implement logic để lấy current user ID từ JWT hoặc session
//            return 1; // Ví dụ
//        }
//    }
//}